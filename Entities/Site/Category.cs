using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PEAS.Entities.Site
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Category
    {
        Hair,
        Photography,
        Dj,
        Makeup,
        Modelling,
        Tattoos,
        Nails,
        Lashes,
        Food,
        Fitness,
    }
}