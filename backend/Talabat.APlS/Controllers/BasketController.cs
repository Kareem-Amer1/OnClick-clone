using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;

namespace Talabat.APlS.Controllers
{
    public class BasketController : APIBaseController
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketController(IBasketRepository basketRepository,IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerBasket>> GetCustomerBasket(string BasketId)
        {
            var Basket = await _basketRepository.GetBasketAsync(BasketId);
            return Basket is null? new CustomerBasket(BasketId): Basket;
        }
        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasketDto Basket)
        {
            var MappedBasket = _mapper.Map<CustomerBasketDto, CustomerBasket>(Basket);
            var CreatedOrUpdatedBasket = await _basketRepository.UpdateBasketAsync(MappedBasket);
            if (CreatedOrUpdatedBasket is null)
            {
                return BadRequest(new ApiResponse(400));
            }
            return Ok(CreatedOrUpdatedBasket);
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteBasket(string BasketId)
        {
            return await _basketRepository.DeleteBasketAsync(BasketId);
        }
    }
}
