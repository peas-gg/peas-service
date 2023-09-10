using System;
namespace PEAS.Models.Account
{
    public class AuthenticateResponse
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public string? InteracEmail { get; set; }

        public required string Phone { get; set; }

        public required string Role { get; set; }

        public required string JwtToken { get; set; }

        public required string RefreshToken { get; set; }
    }
}