using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrescriptoAI.DTOs;
using PrescriptoAI.Services;
using System.Security.Claims;

namespace PrescriptoAI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PharmacistsController : ControllerBase
    {
        private readonly IPharmacistService _pharmacistService;

        public PharmacistsController(IPharmacistService pharmacistService)
        {
            _pharmacistService = pharmacistService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var pharmacistId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var pharmacist = await _pharmacistService.GetPharmacistAsync(pharmacistId);
            return Ok(pharmacist);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] PharmacistUpdateDto updateDto)
        {
            var pharmacistId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _pharmacistService.UpdatePharmacistAsync(pharmacistId, updateDto);
            return NoContent();
        }
    }
}
