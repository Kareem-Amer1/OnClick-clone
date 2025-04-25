using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.APlS.Helpers;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;
using Talabat.Core.Specifications;
using Talabat.Repository;

namespace Talabat.APlS.Controllers
{
    public class ProductsController : APIBaseController
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<ProductType> _typeRepo;
        private readonly IGenericRepository<ProductBrand> _brandRepo;

        public ProductsController(IGenericRepository<Product> ProductRepo,IMapper mapper 
            , IGenericRepository<ProductType> TypeRepo
            , IGenericRepository<ProductBrand> BrandRepo) {
            _productRepo = ProductRepo;
            _mapper = mapper;
            _typeRepo = TypeRepo;
            _brandRepo = BrandRepo;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery]ProductSpecParams Params)
        {
            var Spec = new ProductWithBrandAndTypeSpecictions(Params);
            var products = await _productRepo.GetAllWithSpecAsync(Spec);
            var MappedProducts = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList< ProductToReturnDto> >(products); 
            //var ReturnedObject = new Pagination<ProductToReturnDto>()
            //{
            //    //Count = Spec.Count,
            //    Data = MappedProducts,
            //    PageIndex = Params.PageIndex,
            //    PageSize = Params.PageSize
            //}; 
            var CountSpec = new ProductWithFiltrationForCountAsync(Params);
            var Count = await _productRepo.GetCountWithSpecAsync(CountSpec);
            return Ok(new Pagination<ProductToReturnDto>(Params.PageIndex, Params.PageSize, MappedProducts, Count));
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var Spec = new ProductWithBrandAndTypeSpecictions(id);
            var product = await _productRepo.GetEntityWithSpecAsync(Spec);
            if(product is null) return NotFound(new ApiResponse(404));
            var MappedProduct = _mapper.Map<Product, ProductToReturnDto>(product);
            return Ok(MappedProduct);
        }

        [HttpGet("Types")]
        public async Task<ActionResult<IReadOnlyList<ProductType>>> GetTypes()
        {
            var Types = await _typeRepo.GetAllAsync();
            return Ok(Types);
        }

        [HttpGet("Brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetBrands()
        {
            var Brand = await _brandRepo.GetAllAsync();
            return Ok(Brand);
        }
    }
}
