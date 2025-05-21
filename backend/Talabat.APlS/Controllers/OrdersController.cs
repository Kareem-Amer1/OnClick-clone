using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core;
using Talabat.Core.Entites.Order_Aggregate;
using Talabat.Core.Services;

namespace Talabat.APlS.Controllers
{
    [Authorize]
    public class OrdersController : APIBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IOrderService orderService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _orderService = orderService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpPost] // POST: /api/Orders
        public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var address = _mapper.Map<AddressDto, Address>(orderDto.shipToAddress);

            var order = await _orderService.CreateOrderAsync(buyerEmail, orderDto.BasketId, orderDto.DeliveryMethodId, address);

            if (order is null) return BadRequest(new ApiResponse(400));

            return Ok(order);
        }

        [HttpGet] // GET: /api/Orders
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser()
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var orders = await _orderService.GetOrdersForUserAsync(buyerEmail);

            return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders));
        }

        [HttpGet("{id}")] // GET: /api/Orders/1
        public async Task<ActionResult<OrderToReturnDto>> GetOrderForUser(int id)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var order = await _orderService.GetOrderByIdForUserAsync(buyerEmail, id);

            if (order is null) return NotFound(new ApiResponse(404));

            return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
        }

        [HttpGet("deliveryMethods")] // GET: /api/Orders/deliveryMethods
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();

            return Ok(deliveryMethods);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult<bool>> AuthenticateDeliveryPerson([FromBody] DeliveryAuthDto authDto)
        {
            var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();
            var deliveryPerson = deliveryMethods.FirstOrDefault(d => 
                d.Email == authDto.Email && 
                d.Password == authDto.Password);

            if (deliveryPerson == null)
                return Unauthorized(new ApiResponse(401, "Invalid credentials"));

            return Ok(new { 
                success = true,
                deliveryPerson = new {
                    id = deliveryPerson.Id,
                    email = deliveryPerson.Email,
                    shortName = deliveryPerson.ShortName,
                    address = deliveryPerson.Address,
                    phoneNumber = deliveryPerson.PhoneNumber
                }
            });
        }
    }
}
