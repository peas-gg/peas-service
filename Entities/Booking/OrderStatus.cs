using System.Text.Json.Serialization;

namespace PEAS.Entities.Booking
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        Pending,
        Approved,
        Declined,
        Completed
    }
}