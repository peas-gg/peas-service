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
        public ActionResult<Business> AddBusiness([FromBody] CreateBusiness model)
        {
            var response = _businessService.AddBusiness(Account!, model);
            return Ok(response);
        }

        [Authorize]
        [HttpPatch]
        public ActionResult<Business> UpdateBusiness([FromBody] UpdateBusiness model)
        {
            var response = _businessService.UpdateBusiness(Account!, model);
            return Ok(response);
        }

        [HttpGet("templates")]
        public ActionResult<List<Template>> GetTemplates()
        {
            var response = _businessService.GetTemplate();
            return Ok(response);
        }

        [HttpPost("template")]
        public ActionResult<Template> AddTemplate()
        {
            var response = _businessService.AddTemplate();
            return Ok(response);
        }
    }
}