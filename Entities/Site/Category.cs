using System;
using System.Text.Json.Serialization;

namespace PEAS.Entities.Site
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Category
    {
        Hair,
        Photographer,
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