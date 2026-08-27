using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;

namespace Talabat.APlS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : APIBaseController
    {
        private readonly IGenericRepository<ProductBrand> _brandsRepo;

        public BrandsController(IGenericRepository<ProductBrand> brandsRepo)
        {
            _brandsRepo = brandsRepo;
        }

        // ... existing code ...

        [HttpGet("cities")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetDistinctCities()
        {
            var brands = await _brandsRepo.GetAllAsync();
            var cities = brands.Select(b => b.City).Distinct().OrderBy(c => c).ToList();
            return Ok(cities);
        }
        
        [HttpGet("names")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrandNames()
        {
            var brands = await _brandsRepo.GetAllAsync();
            var brandNames = brands.Select(b => b.Name).Distinct().OrderBy(n => n).ToList();
            return Ok(brandNames);
        }
        
        [HttpGet("top-by-city/{city}/{count}")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetTopBrandsByCity(string city, int count = 4)
        {
            var brands = await _brandsRepo.GetAllAsync();
            
            // Filter by city and order by rating
            var topBrands = brands
                .Where(b => b.City == city)
                .OrderByDescending(b => b.Rating) // Order by actual rating
                .ThenByDescending(b => b.TotalRatings) // Then by number of ratings
                .Take(count)
                .ToList();
                
            return Ok(topBrands);
        }
        
        [HttpGet("countries")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetDistinctCountries()
        {
            var brands = await _brandsRepo.GetAllAsync();
            var countries = brands
                .Where(b => !string.IsNullOrEmpty(b.Country))
                .Select(b => b.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            return Ok(countries);
        }
    }
} 