using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core;
using Talabat.Core.Services;

namespace Talabat.APlS.Controllers
{
    public class DeliveryController : APIBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;

        public DeliveryController(IOrderService orderService, IUnitOfWork unitOfWork)
        {
            _orderService = orderService;
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<bool>> Login([FromBody] DeliveryAuthDto authDto)
        {
            var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();
            var deliveryPerson = deliveryMethods.FirstOrDefault(d => 
                d.Email == authDto.Email && 
                d.Password == authDto.Password);

            if (deliveryPerson == null)
                return Unauthorized(new ApiResponse(401, "Invalid delivery credentials"));

            return Ok(new { 
                success = true,
                deliveryPerson = new {
                    id = deliveryPerson.Id,
                    email = deliveryPerson.Email,
                    shortName = deliveryPerson.ShortName,
                    address = deliveryPerson.Address,
                    phoneNumber = deliveryPerson.PhoneNumber,
                    deliveryTime = deliveryPerson.DeliveryTime,
                    description = deliveryPerson.Description,
                    cost = deliveryPerson.Cost
                }
            });
        }
    }
} 