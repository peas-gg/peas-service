using System.ComponentModel.DataAnnotations;

namespace PEAS.Models.Account
{
    public class AuthenticateRequest
    {
        public required string Email { get; set; }

        public required string Password { get; set; }

        public string? Code { get; set; }
    }
}