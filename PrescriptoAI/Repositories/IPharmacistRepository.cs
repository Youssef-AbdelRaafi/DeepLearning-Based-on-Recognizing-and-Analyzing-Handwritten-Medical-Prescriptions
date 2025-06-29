using PrescriptoAI.Models;

namespace PrescriptoAI.Repositories
{
    public interface IPharmacistRepository
    {
        Task<Pharmacist> GetByIdAsync(int id);
        Task<Pharmacist> GetByEmailAsync(string email);
        Task AddAsync(Pharmacist pharmacist);
        Task UpdateAsync(Pharmacist pharmacist);
        Task<bool> EmailExistsAsync(string email);
    }
}
