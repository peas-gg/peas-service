using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace PEAS.Entities.Authentication
{
    [Owned]
    public class Device
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum Type
        {
            apple,
            android
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Account Account { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public Type DeviceType { get; set; }

        public required string Description { get; set; }

        public required string DeviceToken { get; set; }
    }
}