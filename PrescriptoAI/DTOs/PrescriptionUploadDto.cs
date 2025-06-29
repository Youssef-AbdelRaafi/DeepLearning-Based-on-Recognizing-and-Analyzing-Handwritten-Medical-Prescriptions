using Microsoft.AspNetCore.Http;

namespace PrescriptoAI.DTOs
{
    public class PrescriptionUploadDto
    {
        public IFormFile Image { get; set; } 
    }
}
