using System.ComponentModel.DataAnnotations;

namespace PrescriptoAI.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public string AnalysisResult { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int PharmacistId { get; set; }
        public Pharmacist? Pharmacist { get; set; }
    }
}