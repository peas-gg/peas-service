using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PEAS.Entities.Authentication
{
    public class Account
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required string Phone { get; set; }

        public required string Password { get; set; }

        public int LoginAttempt { get; set; }

        public bool AcceptTerms { get; set; }

        public bool IsLocked { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public Role Role { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }

        public List<RefreshToken>? RefreshTokens { get; set; }

        public List<Device>? Devices { get; set; }

        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }
    }
}