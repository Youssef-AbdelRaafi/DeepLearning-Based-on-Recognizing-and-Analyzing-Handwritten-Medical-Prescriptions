using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrescriptoAI.DTOs;
using PrescriptoAI.Models;
using PrescriptoAI.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrescriptoAI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionsController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] PrescriptionUploadDto uploadDto)
        {
            try
            {
                var pharmacistIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(pharmacistIdClaim) || !int.TryParse(pharmacistIdClaim, out var pharmacistId))
                    return Unauthorized("Invalid user.");

                var prescription = await _prescriptionService.UploadPrescriptionAsync(pharmacistId, uploadDto);

                return Ok(new
                {
                    Message = "Prescription uploaded successfully.",
                    PrescriptionId = prescription.Id,
                    prescription.ImageUrl,
                    prescription.AnalysisResult,
                    prescription.UploadedAt
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetPrescriptions()
        {
            var pharmacistIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(pharmacistIdClaim) || !int.TryParse(pharmacistIdClaim, out var pharmacistId))
            {
                return Unauthorized("Invalid user.");
            }

            var prescriptions = await _prescriptionService.GetPrescriptionsAsync(pharmacistId);
            return Ok(prescriptions);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var pharmacistIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(pharmacistIdClaim) || !int.TryParse(pharmacistIdClaim, out var pharmacistId))
            {
                return Unauthorized("Invalid user.");
            }

            await _prescriptionService.DeletePrescriptionAsync(pharmacistId, id);
            return NoContent();
        }
    }
}