using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PEAS.Entities.Authentication
{
    [Owned]
    public class Device
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Type
        {
            Apple,
            Android
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