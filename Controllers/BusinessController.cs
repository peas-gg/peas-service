using Microsoft.AspNetCore.Mvc;
using PEAS.Entities.Site;
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

        [HttpGet("templates")]
        public ActionResult<List<Template>> GetTemplates()
        {
            var response = _businessService.GetTemplate();
            return Ok(response);
        }
    }
}