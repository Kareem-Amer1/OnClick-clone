using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core.Services;
using Talabat.Service;

namespace Talabat.APlS.Controllers
{
    public class PaymentsController : APIBaseController
    {
        private readonly IPaymentService _paymentService;
        const string endpointSecret = "whsec_3be06b57bd3293bba9dcd4df9033b230d67f7ef243d652a8982f6a5135ecd4ee";
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [Authorize]
        [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasketDto>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var Basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            if (Basket is null) return BadRequest(new ApiResponse(400, "There is a problem with your basket"));
            return Ok(Basket);
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret);
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded) // Fixed namespace issue  
                {
                    await _paymentService.UpdatePaymentIntentToSucceedOrFailed(paymentIntent.Id, true);
                }
                else if (stripeEvent.Type == Stripe.EventTypes.PaymentIntentPaymentFailed) // Fixed namespace issue  
                {
                    await _paymentService.UpdatePaymentIntentToSucceedOrFailed(paymentIntent.Id, false);
                }
                return Ok(); 
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }
}
