using System.Text.Json.Serialization;

namespace PEAS.Entities.Site
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Currency
    {
        USD,
        CAD
    }
}