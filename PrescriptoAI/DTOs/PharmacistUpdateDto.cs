using System.ComponentModel.DataAnnotations;
namespace PrescriptoAI.DTOs
{
    public class PharmacistUpdateDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
