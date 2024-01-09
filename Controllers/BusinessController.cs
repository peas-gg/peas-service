using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Helpers.Utilities;
using PEAS.Models;
using PEAS.Models.Business;
using PEAS.Models.Business.Order;
using PEAS.Models.Business.TimeBlock;
using PEAS.Services;

namespace PEAS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BusinessController : BaseController
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [HttpGet]
        public ActionResult<BusinessResponse> GetBusiness(string sign)
        {
            var response = _businessService.GetBusiness(sign);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("account")]
        public ActionResult<BusinessResponse> GetBusinessAccount()
        {
            var response = _businessService.GetBusiness(Account!);
            return Ok(response);
        }

        [HttpGet("location")]
        public ActionResult<string> GetLocation(double latitude, double longitude)
        {
            var response = _businessService.GetLocation(latitude, longitude);
            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        public ActionResult<BusinessResponse> AddBusiness([FromBody] CreateBusiness model)
        {
            var response = _businessService.AddBusiness(Account!, model);
            return Ok(response);
        }

        [Authorize]
        [HttpPatch]
        public ActionResult<BusinessResponse> UpdateBusiness([FromBody] UpdateBusiness model)
        {
            var response = _businessService.UpdateBusiness(Account!, model);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("block")]
        public ActionResult<BusinessResponse> AddBlock(Guid businessId, [FromBody] Block model)
        {
            var response = _businessService.AddBlock(Account!, businessId, model);
            return Ok(response);
        }

        [Authorize]
        [HttpPatch("block")]
        public ActionResult<BusinessResponse> UpdateBlock(Guid businessId, [FromBody] UpdateBlock model)
        {
            var response = _businessService.UpdateBlock(Account!, businessId, model);
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("block")]
        public ActionResult<BusinessResponse> DeleteBlock(Guid businessId, Guid blockId)
        {
            var response = _businessService.DeleteBlock(Account!, businessId, blockId);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("schedule")]
        public ActionResult<BusinessResponse> SetSchedule(Guid businessId, [FromBody] List<ScheduleModel> model)
        {
            var response = _businessService.SetSchedule(Account!, businessId, model);
            return Ok(response);
        }

        [HttpGet("order")]
        public ActionResult<OrderResponseLite> GetOrder(Guid orderId)
        {
            var response = _businessService.GetOrder(orderId);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("orders")]
        public ActionResult<List<OrderResponse>> GetOrders(Guid businessId)
        {
            var response = _businessService.GetOrders(Account!, businessId);
            return Ok(response);
        }

        [HttpPost("order")]
        public ActionResult<OrderResponse> CreateOrder(Guid businessId, [FromBody] OrderRequest model)
        {
            var response = _businessService.CreateOrder(businessId, model);
            return Ok(response);
        }

        [Authorize]
        [HttpPatch("order")]
        public ActionResult<OrderResponse> UpdateOrder(Guid businessId, [FromBody] UpdateOrderRequest model)
        {
            var response = _businessService.UpdateOrder(Account!, businessId, model);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("time/block")]
        public ActionResult<List<TimeBlockResponse>> GetTimeBlocks(Guid businessId)
        {
            var response = _businessService.GetTimeBlocks(Account!, businessId);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("time/block")]
        public ActionResult<TimeBlockResponse> CreateTimeBlock(Guid businessId, [FromBody] CreateTimeBlock model)
        {
            var response = _businessService.CreateTimeBlock(Account!, businessId, model);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("payment")]
        public ActionResult<OrderResponse> RequestPayment(Guid businessId, [FromBody] PaymentRequest model)
        {
            var response = _businessService.RequestPayment(Account!, businessId, model);
            return Ok(response);
        }

        [HttpGet("availability")]
        public ActionResult<List<DateRange>> GetAvailability(Guid businessId, Guid blockId, DateTime date)
        {
            var response = _businessService.GetAvailablity(businessId, blockId, date);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("customers")]
        public ActionResult<List<Customer>> GetCustomers(Guid businessId)
        {
            var response = _businessService.GetCustomers(Account!, businessId);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("wallet")]
        public ActionResult<WalletResponse> GetWallet(Guid businessId)
        {
            var response = _businessService.GetWallet(Account!, businessId);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("wallet/withdraw")]
        public ActionResult<WalletResponse> Withdraw(Guid businessId)
        {
            var response = _businessService.Withdraw(Account!, businessId);
            return Ok(response);
        }

        [Helpers.Authorize(Role.Admin)]
        [HttpPost("withdraw/complete")]
        public ActionResult<EmptyResponse> CompleteWithdraw(Guid withdrawalId)
        {
            var response = _businessService.CompleteWithdraw(withdrawalId);
            return Ok(response);
        }

        [HttpGet("templates")]
        public ActionResult<List<TemplateResponse>> GetTemplates()
        {
            var response = _businessService.GetTemplates();
            return Ok(response);
        }

        [Helpers.Authorize(Role.Admin)]
        [HttpPost("template")]
        public ActionResult<TemplateResponse> AddTemplate([FromBody] CreateTemplate model)
        {
            var response = _businessService.AddTemplate(model);
            return Ok(response);
        }

        [Helpers.Authorize(Role.Admin)]
        [HttpDelete("template")]
        public ActionResult DeleteTemplate(Guid id)
        {
            _businessService.DeleteTemplate(id);
            return Ok();
        }

        [HttpGet("colours")]
        public ActionResult<Dictionary<string, string>> GetColours()
        {
            var response = _businessService.GetColours();
            return Ok(response);
        }
    }
}