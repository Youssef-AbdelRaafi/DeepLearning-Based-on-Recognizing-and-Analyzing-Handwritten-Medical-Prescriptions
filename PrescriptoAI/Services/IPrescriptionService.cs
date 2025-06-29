using PrescriptoAI.DTOs;
using PrescriptoAI.Models;
using Microsoft.AspNetCore.Http;
namespace PrescriptoAI.Services
{
    public interface IPrescriptionService
    {
        Task<Prescription> UploadPrescriptionAsync(int pharmacistId, PrescriptionUploadDto uploadDto);
        Task<List<Prescription>> GetPrescriptionsAsync(int pharmacistId);
        Task DeletePrescriptionAsync(int pharmacistId, int prescriptionId);
    }
}