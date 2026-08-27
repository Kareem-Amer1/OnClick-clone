using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;

namespace Talabat.APlS.Controllers
{
    public class BrandAuthController : APIBaseController
    {
        private readonly IGenericRepository<ProductBrand> _brandsRepo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public BrandAuthController(
            IGenericRepository<ProductBrand> brandsRepo,
            IConfiguration config,
            IMapper mapper)
        {
            _brandsRepo = brandsRepo;
            _config = config;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<ActionResult<BrandToReturnDto>> Login(BrandLoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                    return BadRequest(new ApiResponse(400, "Email and password are required"));

                var spec = new Specifications.BrandByEmailSpecification(loginDto.Email);
                var brand = await _brandsRepo.GetEntityWithSpecAsync(spec);

                if (brand == null)
                    return Unauthorized(new ApiResponse(401, "Invalid email"));

                if (string.IsNullOrEmpty(brand.Password) || brand.Password != loginDto.Password)
                    return Unauthorized(new ApiResponse(401, "Invalid password"));

                var brandToReturn = _mapper.Map<ProductBrand, BrandToReturnDto>(brand);
                brandToReturn.Token = GenerateJwtToken(brand);

                if (string.IsNullOrEmpty(brandToReturn.Token))
                    return StatusCode(500, new ApiResponse(500, "Error generating authentication token"));

                return brandToReturn;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<ActionResult<BrandToReturnDto>> UpdateBrand(UpdateBrandDto updateDto)
        {
            try
            {
                // Get brand id from claims
                var brandIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (brandIdClaim == null)
                    return Unauthorized(new ApiResponse(401, "Unauthorized"));

                if (!int.TryParse(brandIdClaim.Value, out int brandId))
                    return BadRequest(new ApiResponse(400, "Invalid brand ID"));

                var brand = await _brandsRepo.GetByIdAsync(brandId);

                if (brand == null)
                    return NotFound(new ApiResponse(404, "Brand not found"));

                // Update brand properties
                brand.Name = updateDto.Name ?? brand.Name;
                brand.Street = updateDto.Street ?? brand.Street;
                brand.City = updateDto.City ?? brand.City;
                brand.Country = updateDto.Country ?? brand.Country;
                brand.Description = updateDto.Description ?? brand.Description;

                if (!string.IsNullOrEmpty(updateDto.OpeningTime))
                {
                    if (TimeSpan.TryParse(updateDto.OpeningTime, out TimeSpan openingTime))
                        brand.OpeningTime = openingTime;
                }

                if (!string.IsNullOrEmpty(updateDto.ClosingTime))
                {
                    if (TimeSpan.TryParse(updateDto.ClosingTime, out TimeSpan closingTime))
                        brand.ClosingTime = closingTime;
                }

                _brandsRepo.Update(brand);
                await _brandsRepo.SaveChangesAsync();

                var brandToReturn = _mapper.Map<ProductBrand, BrandToReturnDto>(brand);
                brandToReturn.Token = GenerateJwtToken(brand);

                return brandToReturn;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<BrandToReturnDto>> GetCurrentBrand()
        {
            try
            {
                var brandIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (brandIdClaim == null)
                    return Unauthorized(new ApiResponse(401, "Unauthorized"));

                if (!int.TryParse(brandIdClaim.Value, out int brandId))
                    return BadRequest(new ApiResponse(400, "Invalid brand ID"));

                var brand = await _brandsRepo.GetByIdAsync(brandId);

                if (brand == null)
                    return NotFound(new ApiResponse(404, "Brand not found"));

                var brandToReturn = _mapper.Map<ProductBrand, BrandToReturnDto>(brand);
                brandToReturn.Token = GenerateJwtToken(brand);

                return brandToReturn;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, $"An error occurred: {ex.Message}"));
            }
        }

        private string GenerateJwtToken(ProductBrand brand)
        {
            if (brand == null)
                return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, brand.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, brand.Id.ToString()),
                new Claim(ClaimTypes.Name, brand.Name ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:ValidIssuer"],
                Audience = _config["JWT:ValidAudience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
} 