using Microsoft.AspNetCore.Mvc;
using PEAS.Models.Business;
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
        public ActionResult<string> RequestPayment(Guid orderId)
        {
            var response = _paymentService.RequestPayment(orderId);
            return Ok(response);
        }
    }
}