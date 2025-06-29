using PrescriptoAI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrescriptoAI.Repositories
{
    public interface IPrescriptionRepository
    {
        Task AddPrescriptionAsync(Prescription prescription);
        Task<List<Prescription>> GetPrescriptionsByPharmacistIdAsync(int pharmacistId);
        Task<Prescription> GetPrescriptionByIdAsync(int id);
        Task DeletePrescriptionAsync(Prescription prescription);
        Task SaveChangesAsync();
    }
}