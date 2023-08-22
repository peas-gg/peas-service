using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEAS.Entities.Site;
using PEAS.Models.Business;
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

        [HttpGet("templates")]
        public ActionResult<List<TemplateResponse>> GetTemplates()
        {
            var response = _businessService.GetTemplates();
            return Ok(response);
        }

        [HttpPost("template")]
        public ActionResult<TemplateResponse> AddTemplate([FromBody] CreateTemplate model)
        {
            var response = _businessService.AddTemplate(model);
            return Ok(response);
        }

        [HttpDelete("template")]
        public ActionResult DeleteTemplate(Guid id)
        {
            _businessService.DeleteTemplate(id);
            return Ok();
        }
    }
}