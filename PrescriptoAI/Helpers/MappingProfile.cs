using AutoMapper;
using PrescriptoAI.DTOs;
using PrescriptoAI.Models;

namespace PrescriptoAI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map RegisterDto to Pharmacist
            CreateMap<RegisterDto, Pharmacist>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => new PasswordHasher().HashPassword(src.Password)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Map PharmacistUpdateDto to Pharmacist
            CreateMap<PharmacistUpdateDto, Pharmacist>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Map Pharmacist to a simplified response (if needed)
            CreateMap<Pharmacist, Pharmacist>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Ignore password hash in responses
        }
    }
}
