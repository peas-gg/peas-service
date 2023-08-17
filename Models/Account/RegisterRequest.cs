using System.ComponentModel.DataAnnotations;

namespace PEAS.Models.Account
{
    public class RegisterRequest
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required string Phone { get; set; }

        public required string PasswordText { get; set; }

        public required string OtpCode { get; set; }

        public bool AcceptTerms { get; set; }
    }
}