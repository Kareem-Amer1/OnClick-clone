using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core.Services;

namespace Talabat.APlS.Controllers
{
    public class PaymentsController : APIBaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<CustomerBasketDto>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var Basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            if (Basket is null) return BadRequest(new ApiResponse(400, "There is a problem with your basket"));
            return Ok(Basket);
        }
    }
}
