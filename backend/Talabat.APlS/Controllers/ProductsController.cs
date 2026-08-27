using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.APlS.Helpers;
using Talabat.Core;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;
using Talabat.Core.Specifications;
using Talabat.Repository;
using System.Security.Claims;

namespace Talabat.APlS.Controllers
{
    public class ProductsController : APIBaseController
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<ProductType> _typeRepo;
        private readonly IGenericRepository<ProductBrand> _brandRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(IGenericRepository<Product> ProductRepo,IMapper mapper 
            , IGenericRepository<ProductType> TypeRepo
            , IGenericRepository<ProductBrand> BrandRepo
            , IUnitOfWork unitOfWork) {
            _productRepo = ProductRepo;
            _mapper = mapper;
            _typeRepo = TypeRepo;
            _brandRepo = BrandRepo;
            _unitOfWork = unitOfWork;
        }
        [CachedAttribute(300)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var spec = new ProductWithBrandAndTypeSpecictions(id);

            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);

            if (product == null) return NotFound(new ApiResponse(404));

            return _mapper.Map<Product, ProductToReturnDto>(product);
        }

        [HttpGet("Types")]
        public async Task<ActionResult<IReadOnlyList<ProductType>>> GetTypes()
        {
            var Types = await _typeRepo.GetAllAsync();
            return Ok(Types);
        }

        [HttpGet("Brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrandDto>>> GetBrands()
        {
            var brands = await _brandRepo.GetAllAsync();
            var brandDtos = _mapper.Map<IReadOnlyList<ProductBrand>, IReadOnlyList<ProductBrandDto>>(brands);
            return Ok(brandDtos);
        }

        [HttpGet("Brands/{id}")]
        public async Task<ActionResult<ProductBrandDto>> GetBrand(int id)
        {
            var brand = await _brandRepo.GetByIdAsync(id);
            if (brand == null) return NotFound(new ApiResponse(404));
            var brandDto = _mapper.Map<ProductBrand, ProductBrandDto>(brand);
            return Ok(brandDto);
        }

        [HttpGet("{id}/comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ProductCommentDto>>> GetProductComments(int id)
        {
            var comments = await _unitOfWork.Repository<ProductComment>()
                .GetAllWithSpecAsync(new BaseSpeceifications<ProductComment>(x => x.ProductId == id));

            return Ok(_mapper.Map<IReadOnlyList<ProductComment>, IReadOnlyList<ProductCommentDto>>(comments));
        }

        [Authorize]
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<ProductCommentDto>> AddProductComment(int id, CreateProductCommentDto commentDto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];

            var comment = new ProductComment
            {
                ProductId = id,
                UserEmail = email,
                UserName = name,
                Comment = commentDto.Comment,
                Rating = commentDto.Rating
            };

            await _unitOfWork.Repository<ProductComment>().AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetProductComments), new { id }, _mapper.Map<ProductComment, ProductCommentDto>(comment));
        }
    }
}
