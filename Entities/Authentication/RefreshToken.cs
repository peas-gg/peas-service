using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PEAS.Entities.Authentication
{
    [Owned]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        public required Account Account { get; set; }

        public required string Token { get; set; }

        public DateTime Expires { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
        
        public required string CreatedByIp { get; set; }
        public required string RevokedByIp { get; set; }
        public required string ReplacedByToken { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}