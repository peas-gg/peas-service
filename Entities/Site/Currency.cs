using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PEAS.Entities.Site
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Currency
    {
        USD,
        CAD
    }
}