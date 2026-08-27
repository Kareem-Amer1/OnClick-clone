using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;
using Talabat.Core.Specifications;

namespace Talabat.APlS.Controllers
{
    // Temporarily remove Authorize attribute for testing
    public class BrandProductsController : APIBaseController
    {
        private readonly IGenericRepository<Product> _productsRepo;
        private readonly IGenericRepository<ProductBrand> _brandsRepo;
        private readonly IGenericRepository<ProductType> _typesRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandProductsController> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public BrandProductsController(
            IGenericRepository<Product> productsRepo,
            IGenericRepository<ProductBrand> brandsRepo,
            IGenericRepository<ProductType> typesRepo,
            IMapper mapper,
            ILogger<BrandProductsController> logger,
            IMemoryCache cache)
        {
            _productsRepo = productsRepo;
            _brandsRepo = brandsRepo;
            _typesRepo = typesRepo;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetBrandProducts()
        {
            try
            {
                // Get brand id from claims
                var brandIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (brandIdClaim == null)
                    return Unauthorized(new ApiResponse(401, "Unauthorized"));

                int brandId = int.Parse(brandIdClaim.Value);

                // Get brand products using a direct approach
                return await GetProductsByBrandIdInternal(brandId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand products");
                return StatusCode(500, new ApiResponse(500, "An error occurred while processing your request"));
            }
        }

        [HttpGet("by-brand/{brandId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetProductsByBrandId(int brandId)
        {
            try
            {
                return await GetProductsByBrandIdInternal(brandId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for brand {BrandId}", brandId);
                return StatusCode(500, new ApiResponse(500, "An error occurred while processing your request"));
            }
        }
        
        private async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetProductsByBrandIdInternal(int brandId)
        {
            string cacheKey = $"brand_products_{brandId}";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out ActionResult<IReadOnlyList<ProductToReturnDto>> cachedResult))
            {
                return cachedResult;
            }
            
            // Check if brand exists
            var brand = await _brandsRepo.GetByIdAsync(brandId);
            if (brand == null)
                return NotFound(new ApiResponse(404, $"Brand with ID {brandId} not found"));

            // Get all products without pagination for now
            var spec = new ProductWithBrandAndTypeSpecictions(new ProductSpecParams { BrandId = brandId, PageSize = 100 });
            var products = await _productsRepo.GetAllWithSpecAsync(spec);

            // Debug information
            var productCount = products.Count;
            var productList = string.Join(", ", products.Select(p => p.Name));
            var productTypeIds = string.Join(", ", products.Select(p => p.ProductTypeId));

            // Return with additional debug info
            var result = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);
            
            var response = Ok(new { 
                products = result,
                debugInfo = new {
                    brandId = brandId,
                    productCount = productCount,
                    productNames = productList,
                    productTypeIds = productTypeIds
                }
            });
            
            // Store in cache
            _cache.Set(cacheKey, response, _cacheExpiration);
            
            return response;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductToReturnDto>> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            try
            {
                // Get brand id from claims
                var brandIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (brandIdClaim == null)
                    return Unauthorized(new ApiResponse(401, "Unauthorized"));

                int brandId = int.Parse(brandIdClaim.Value);

                // Create new product
                var product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    PictureUrl = productDto.PictureUrl,
                    ProductBrandId = brandId,
                    ProductTypeId = productDto.ProductTypeId
                };

                await _productsRepo.AddAsync(product);
                await _productsRepo.SaveChangesAsync();

                // Map to return DTO without fetching again
                var productToReturn = _mapper.Map<Product, ProductToReturnDto>(product);
                
                // Add brand name
                var brand = await _brandsRepo.GetByIdAsync(brandId);
                if (brand != null)
                    productToReturn.ProductBrand = brand.Name;
                
                // Add product type name
                var type = await _typesRepo.GetByIdAsync(product.ProductTypeId);
                if (type != null)
                    productToReturn.ProductType = type.Name;

                // Invalidate cache after product creation
                _cache.Remove($"brand_products_{brandId}");

                return Ok(productToReturn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new ApiResponse(500, "An error occurred while creating the product"));
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductToReturnDto>> UpdateProduct(int id, [FromBody] ProductUpdateDto productDto)
        {
            try 
            {
                // Get brand id from claims
                var brandIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (brandIdClaim == null)
                    return Unauthorized(new ApiResponse(401, "Unauthorized"));

                int brandId = int.Parse(brandIdClaim.Value);

                // Get product
                var product = await _productsRepo.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse(404, "Product not found"));

                // Verify product belongs to brand
                if (product.ProductBrandId != brandId)
                    return Forbid();

                // Update product fields
                product.Name = productDto.Name ?? product.Name;
                product.Description = productDto.Description ?? product.Description;
                product.Price = productDto.Price;
                product.PictureUrl = string.IsNullOrEmpty(productDto.PictureUrl) ? product.PictureUrl : productDto.PictureUrl;
                product.ProductTypeId = productDto.ProductTypeId > 0 ? productDto.ProductTypeId : product.ProductTypeId;

                _productsRepo.Update(product);
                await _productsRepo.SaveChangesAsync();

                // Map to return DTO without fetching again
                var productToReturn = _mapper.Map<Product, ProductToReturnDto>(product);
                
                // Add brand name
                var brand = await _brandsRepo.GetByIdAsync(brandId);
                if (brand != null)
                    productToReturn.ProductBrand = brand.Name;
                
                // Add product type name
                var type = await _typesRepo.GetByIdAsync(product.ProductTypeId);
                if (type != null)
                    productToReturn.ProductType = type.Name;
                
                // Invalidate cache after product update
                _cache.Remove($"brand_products_{brandId}");

                return Ok(productToReturn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, new ApiResponse(500, "An error occurred while updating the product"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                // Get brand id from claims
                var brandIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (brandIdClaim == null)
                    return Unauthorized(new ApiResponse(401, "Unauthorized"));

                int brandId = int.Parse(brandIdClaim.Value);

                // Get product
                var product = await _productsRepo.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse(404, "Product not found"));

                // Verify product belongs to brand
                if (product.ProductBrandId != brandId)
                    return Forbid();

                // Delete product
                _productsRepo.Delete(product);
                await _productsRepo.SaveChangesAsync();

                // Invalidate cache after product deletion
                _cache.Remove($"brand_products_{brandId}");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, new ApiResponse(500, "An error occurred while deleting the product"));
            }
        }
    }
} 