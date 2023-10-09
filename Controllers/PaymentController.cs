using Microsoft.AspNetCore.Mvc;
using PEAS.Services;
using Stripe;

namespace PEAS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public ActionResult<string> StartPayment(Guid orderId, int tip)
        {
            var response = _paymentService.StartPayment(orderId, tip);
            return Ok(response);
        }

        [HttpPost("complete")]
        public async void CompletePayment()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ParseEvent(json);

            // Handle the event
            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                _paymentService.CompletePayment(paymentIntent!);
            }
        }
    }
}