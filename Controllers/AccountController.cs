using Microsoft.AspNetCore.Mvc;
using PEAS.Entities.Authentication;
using PEAS.Helpers;
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

        [HttpPost("authenticate")]
        public ActionResult<EmptyResponse> Authenticate([FromBody] AuthenticateRequest model)
        {
            _ = _accountService.Authenticate(model, ipAddress());
            return Ok(new EmptyResponse());
        }

        [HttpPost("authenticateWithCode")]
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

        [HttpPost("refresh")]
        public ActionResult<AuthenticateResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _accountService.RefreshToken(refreshToken!, ipAddress());
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

        [Authorize]
        [HttpPatch("interacEmail")]
        public ActionResult<string> SetInteracEmail(string email)
        {
            var response = _accountService.SetInteracEmail(Account!, email);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("device")]
        public ActionResult<EmptyResponse> DeviceToken(string token, Device.Type deviceType)
        {
            var response = _accountService.UpdateDeviceToken(Account!,deviceType, token);
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