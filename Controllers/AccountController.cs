using Microsoft.AspNetCore.Mvc;
using PEAS.Models.Account;
using PEAS.Services;

namespace PEAS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("authenticate")]
        public ActionResult<AuthenticateResponse> Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = _accountService.Authenticate(model, ipAddress());
            return Ok(response);
        }

        [HttpPost("register")]
        public ActionResult<AuthenticateResponse> Register([FromBody] RegisterRequest model)
        {
            var response = _accountService.Register(model, ipAddress());
            return Ok(response);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"]!;
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString()!;
        }
    }
}