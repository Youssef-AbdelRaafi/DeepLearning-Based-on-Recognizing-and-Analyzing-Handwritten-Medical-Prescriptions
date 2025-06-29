using Microsoft.EntityFrameworkCore;
using PrescriptoAI.Data;
using PrescriptoAI.Models;

namespace PrescriptoAI.Repositories
{
    public class PharmacistRepository : IPharmacistRepository
    {
        private readonly PrescriptionDbContext _context;

        public PharmacistRepository(PrescriptionDbContext context)
        {
            _context = context;
        }

        public async Task<Pharmacist> GetByIdAsync(int id)
        {
            return await _context.Pharmacists
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pharmacist> GetByEmailAsync(string email)
        {
            return await _context.Pharmacists
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task AddAsync(Pharmacist pharmacist)
        {
            await _context.Pharmacists.AddAsync(pharmacist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pharmacist pharmacist)
        {
            _context.Pharmacists.Update(pharmacist);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Pharmacists.AnyAsync(p => p.Email == email);
        }
    }
}
