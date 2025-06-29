using Microsoft.EntityFrameworkCore;
using PrescriptoAI.Data;
using PrescriptoAI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrescriptoAI.Repositories
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly PrescriptionDbContext _context;

        public PrescriptionRepository(PrescriptionDbContext context)
        {
            _context = context;
        }

        public async Task AddPrescriptionAsync(Prescription prescription)
        {
            await _context.Prescriptions.AddAsync(prescription);
        }

        public async Task<List<Prescription>> GetPrescriptionsByPharmacistIdAsync(int pharmacistId)
        {
            return await _context.Prescriptions
                .Where(p => p.PharmacistId == pharmacistId)
                .ToListAsync();
        }

        public async Task<Prescription> GetPrescriptionByIdAsync(int id)
        {
            return await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task DeletePrescriptionAsync(Prescription prescription)
        {
            _context.Prescriptions.Remove(prescription);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}