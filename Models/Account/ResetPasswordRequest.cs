using System;
namespace PEAS.Models.Account
{
    public class ResetPasswordRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Code { get; set; }
    }
}