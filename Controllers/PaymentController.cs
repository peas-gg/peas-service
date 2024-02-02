using Microsoft.AspNetCore.Mvc;
using PEAS.Models;
using PEAS.Services;

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
            //var response = _paymentService.StartPayment(orderId, tip);
            return NotFound();
        }

        [HttpPost("complete")]
        public ActionResult<EmptyResponse> CompletePayment()
        {
            var response = _paymentService.CompletePayment(HttpContext.Request).Result;
            return Ok(response);
        }
    }
}