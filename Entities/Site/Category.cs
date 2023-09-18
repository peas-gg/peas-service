﻿using System;
using System.Text.Json.Serialization;

namespace PEAS.Entities.Site
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
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