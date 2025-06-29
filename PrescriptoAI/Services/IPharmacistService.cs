using PrescriptoAI.DTOs;
using PrescriptoAI.Models;

namespace PrescriptoAI.Services
{
    public interface IPharmacistService
    {
        Task<Pharmacist> GetPharmacistAsync(int id);
        Task UpdatePharmacistAsync(int id, PharmacistUpdateDto updateDto);
    }
}
