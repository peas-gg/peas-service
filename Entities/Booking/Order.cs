using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using PEAS.Entities.Site;

namespace PEAS.Entities.Booking
{
    public class Order
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum Status
        {
            Pending,
            Approved,
            Declined,
            Completed
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Business Business { get; set; }

        public required Block Block { get; set; }

        public required Customer Customer { get; set; }

        public required Currency Currency { get; set; }

        public required int Price { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public required Uri Image { get; set; }

        public string? Note { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }

        public required Status OrderStatus { get; set; }

        public List<Payment>? Payments { get; set; }

        public required DateTime Created { get; set; }

        public required DateTime LastUpdated { get; set; }

        [Timestamp]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public byte[] Version { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}