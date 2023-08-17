﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PEAS.Entities.Authentication
{
    [Owned]
    public class Device
    {
        public enum Type
        {
            apple,
            android
        }

        [Key]
        public int Id { get; set; }

        public required Account Account { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public Type DeviceType { get; set; }

        public required string Description { get; set; }

        public required string DeviceToken { get; set; }
    }
}