using Microsoft.IdentityModel.Tokens;
using PrescriptoAI.DTOs;
using PrescriptoAI.Models;
using PrescriptoAI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PrescriptoAI.Helpers;
using System;
using AutoMapper;

namespace PrescriptoAI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPharmacistRepository _pharmacistRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(IPharmacistRepository pharmacistRepository, IConfiguration configuration, IMapper mapper)
        {
            _pharmacistRepository = pharmacistRepository;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            if (await _pharmacistRepository.EmailExistsAsync(registerDto.Email))
                throw new Exception("Email already exists.");

            var pharmacist = _mapper.Map<Pharmacist>(registerDto); // AutoMapper handles the mapping
            await _pharmacistRepository.AddAsync(pharmacist);
            return GenerateJwtToken(pharmacist);
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var pharmacist = await _pharmacistRepository.GetByEmailAsync(loginDto.Email);
            if (pharmacist == null || !PasswordHasher.VerifyPassword(loginDto.Password, pharmacist.PasswordHash))
                throw new Exception("Invalid email or password.");

            return GenerateJwtToken(pharmacist);
        }

        private string GenerateJwtToken(Pharmacist pharmacist)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, pharmacist.Id.ToString()),
                new Claim(ClaimTypes.Email, pharmacist.Email),
                new Claim(ClaimTypes.Name, pharmacist.FullName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
