using PrescriptoAI.DTOs;
using PrescriptoAI.Models;
using PrescriptoAI.Repositories;

namespace PrescriptoAI.Services
{
    public class PharmacistService : IPharmacistService
    {
        private readonly IPharmacistRepository _pharmacistRepository;

        public PharmacistService(IPharmacistRepository pharmacistRepository)
        {
            _pharmacistRepository = pharmacistRepository;
        }

        public async Task<Pharmacist> GetPharmacistAsync(int id)
        {
            var pharmacist = await _pharmacistRepository.GetByIdAsync(id);
            if (pharmacist == null)
                throw new Exception("Pharmacist not found.");
            return pharmacist;
        }

        public async Task UpdatePharmacistAsync(int id, PharmacistUpdateDto updateDto)
        {
            var pharmacist = await _pharmacistRepository.GetByIdAsync(id);
            if (pharmacist == null)
                throw new Exception("Pharmacist not found.");

            pharmacist.FullName = updateDto.FullName;
            pharmacist.Email = updateDto.Email;
            pharmacist.UpdatedAt = DateTime.UtcNow;

            await _pharmacistRepository.UpdateAsync(pharmacist);
        }
    }
}
