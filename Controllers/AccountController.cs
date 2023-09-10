using Microsoft.AspNetCore.Mvc;
using PEAS.Models;
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

        [HttpGet("authenticate")]
        public ActionResult<EmptyResponse> Authenticate([FromBody] AuthenticateRequest model)
        {
            _ = _accountService.Authenticate(model, ipAddress());
            return Ok(new EmptyResponse());
        }

        [HttpPost("authenticate")]
        public ActionResult<AuthenticateResponse> AuthenticateWithCode([FromBody] AuthenticateRequest model)
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

        [HttpGet("validatePhone")]
        public ActionResult<string?> ValidatePhoneNumber(string number)
        {
            var response = _accountService.ValidatePhoneNumber(number);
            return Ok(response);
        }

        [HttpGet("code")]
        public ActionResult<EmptyResponse> RequestVerificationCode(string number)
        {
            var response = _accountService.RequestVerificationCode(number);
            return Ok(response);
        }

        [HttpGet("password/reset")]
        public ActionResult<EmptyResponse> RequestPasswordReset(string email)
        {
            var response = _accountService.RequestPasswordReset(email);
            return Ok(response);
        }

        [HttpPost("password/reset")]
        public ActionResult<EmptyResponse> RequestPasswordReset([FromBody] ResetPasswordRequest model)
        {
            var response = _accountService.ResetPassword(model);
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