using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrescriptoAI.DTOs;
using PrescriptoAI.Models;
using PrescriptoAI.Repositories;

namespace PrescriptoAI.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly string _apiKey;
        private readonly ILogger<PrescriptionService> _logger;
        private readonly HttpClient _httpClient;

        public PrescriptionService(
            IPrescriptionRepository prescriptionRepository,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<PrescriptionService> logger)
        {
            _prescriptionRepository = prescriptionRepository;
            _environment = environment;
            _apiKey = configuration["RoboflowConfig:ApiKey"];
            _logger = logger;
            _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("RoboflowConfig:ApiKey is not configured.");
                throw new InvalidOperationException("RoboflowConfig:ApiKey is not configured.");
            }
        }

        public async Task<Prescription> UploadPrescriptionAsync(int pharmacistId, PrescriptionUploadDto uploadDto)
        {
            if (uploadDto.Image == null || uploadDto.Image.Length == 0)
                throw new ArgumentException("No image file provided.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadDto.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the image to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadDto.Image.CopyToAsync(stream);
            }

            string responseContent;
            try
            {
                // Construct the public URL for the image
                string imageUrl = $"http://localhost:5035/Uploads/{fileName}";

                // Convert the image to Base64 as a fallback
                byte[] imageArray = await File.ReadAllBytesAsync(filePath);
                string imageBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(imageArray)}";

                // Upload image to Roboflow and retrieve the result
                responseContent = await UploadToRoboflowAsync(imageUrl, fileName, imageBase64);
            }
            catch (Exception ex)
            {
                if (File.Exists(filePath)) File.Delete(filePath); // Clean up if error occurs
                throw new ArgumentException($"Failed to upload prescription image: {ex.Message}", ex);
            }

            // Create a Prescription entity with the image URL and result from Roboflow
            var prescription = new Prescription
            {
                ImageUrl = $"/Uploads/{fileName}",
                PharmacistId = pharmacistId,
                AnalysisResult = responseContent,
                UploadedAt = DateTime.UtcNow
            };

            // Save the prescription to the database
            await _prescriptionRepository.AddPrescriptionAsync(prescription);
            await _prescriptionRepository.SaveChangesAsync();

            return prescription;
        }

        private async Task<string> UploadToRoboflowAsync(string imageUrl, string imageName, string imageBase64)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new ArgumentException("Roboflow API key is missing or empty.");

            if (string.IsNullOrEmpty(imageBase64))
                throw new ArgumentException("Image Base64 data is empty.");

            try
            {
                // Construct the URL
                string uploadUrl = "https://serverless.roboflow.com/infer/workflows/myprojectnaeem/detect-count-and-visualize";

                // Prepare the JSON payload
                var payload = new
                {
                    api_key = _apiKey,
                    inputs = new
                    {
                        image = new
                        {
                            type = "base64", // Use base64 since URL may not be accessible to Roboflow
                            value = imageBase64
                        }
                    }
                };
                string jsonPayload = JsonSerializer.Serialize(payload);
                _logger.LogInformation("Roboflow Request Payload: {Payload}", jsonPayload);

                // Configure Request
                using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "PrescriptoAI/1.0");

                // Send Request
                var response = await _httpClient.PostAsync(uploadUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Roboflow API returned error. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);
                    throw new HttpRequestException($"Roboflow API request failed with status {response.StatusCode}: {responseContent}");
                }

                _logger.LogInformation("Raw Roboflow Response: {Response}", responseContent);

                // Parse the response
                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;

                string imageResult = null;
                if (root.TryGetProperty("outputs", out var outputsElement) && outputsElement.ValueKind == JsonValueKind.Array && outputsElement.GetArrayLength() > 0)
                {
                    var firstOutput = outputsElement[0]; // Access the first element of the outputs array

                    // Try to get the detection image from output_image
                    if (firstOutput.TryGetProperty("output_image", out var outputImageElement))
                    {
                        if (outputImageElement.TryGetProperty("value", out var valueElement))
                        {
                            imageResult = valueElement.GetString();
                            if (!string.IsNullOrEmpty(imageResult))
                            {
                                // Ensure the Base64 string is properly formatted for display
                                if (!imageResult.StartsWith("data:image/jpeg;base64,"))
                                {
                                    imageResult = $"data:image/jpeg;base64,{imageResult}";
                                }
                            }
                        }
                    }

                    // If no output_image, check for predictions and fallback to original image
                    if (string.IsNullOrEmpty(imageResult) && firstOutput.TryGetProperty("predictions", out var predictionsElement))
                    {
                        if (predictionsElement.TryGetProperty("predictions", out var predictionsArray) &&
                            predictionsArray.ValueKind == JsonValueKind.Array && predictionsArray.GetArrayLength() > 0)
                        {
                            // Fallback to original image if predictions exist but no detection image
                            imageResult = imageBase64;
                        }
                    }
                }

                // Fallback to original image Base64 if no result is found, or return error message
                imageResult ??= root.TryGetProperty("error", out var errorElement)
                    ? $"Error: {errorElement.GetString()}"
                    : imageBase64;

                _logger.LogInformation("Extracted Detection Image Result: {ImageResult}", imageResult);
                return imageResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload to Roboflow for image: {ImageName}", imageName);
                return imageBase64; // Fallback to original image Base64 in case of error
            }
        }

        public async Task<List<Prescription>> GetPrescriptionsAsync(int pharmacistId)
        {
            return await _prescriptionRepository.GetPrescriptionsByPharmacistIdAsync(pharmacistId);
        }

        public async Task DeletePrescriptionAsync(int pharmacistId, int prescriptionId)
        {
            var prescription = (await _prescriptionRepository.GetPrescriptionsByPharmacistIdAsync(pharmacistId))
                .FirstOrDefault(p => p.Id == prescriptionId);

            if (prescription == null)
                throw new Exception("Prescription not found.");

            var filePath = Path.Combine(_environment.WebRootPath, prescription.ImageUrl.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);

            await _prescriptionRepository.DeletePrescriptionAsync(prescription);
            await _prescriptionRepository.SaveChangesAsync();
        }
    }
}