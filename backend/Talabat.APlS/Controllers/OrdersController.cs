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

    public class OrdersController : APIBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IOrderService orderService, IMapper mapper,IUnitOfWork unitOfWork)
        {
            _orderService = orderService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)] // Fixed the incorrect usage of StatusCode
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
        {
            var BuyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var MappedAddress = _mapper.Map<AddressDto, Address>(orderDto.shipToAddress);
            var Order = await _orderService.CreateOrderAsync(BuyerEmail, orderDto.BasketId, orderDto.DeliveryMethodId, MappedAddress);
            if (Order is null) return BadRequest(new ApiResponse(400, "There is a Problem With Your Order"));
            return Ok(Order);
        }
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)] // Fixed the incorrect usage of StatusCode
        [ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDto>), StatusCodes.Status200OK)] // Fixed the incorrect usage of StatusCode 
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> CreateOrdersForUser()
        {
            var BuyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var Orders = await _orderService.GetOrdersForSpecificUserAsync(BuyerEmail);
            if (Orders is null) return NotFound(new ApiResponse(404, "There is no orders for this user"));
            var MapperOrders = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(Orders);
            return Ok(MapperOrders);
        }
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)] // Fixed the incorrect usage of StatusCode
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderByIdForUser(int id)
        {
            var BuyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var order = await _orderService.GetOrderByIdForSpecificUserAsync(BuyerEmail, id);
            if (order is null) return NotFound(new ApiResponse(404, $"There is no orders with {id} for this user"));
            var MapperOrder = _mapper.Map<Order, OrderToReturnDto>(order);
            return Ok(MapperOrder);
        }

        [HttpGet("DeliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            var DeliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
            return Ok(DeliveryMethods);
        }

    }
}
