﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace PEAS.Entities.Site
{
    [Owned]
    public class Block
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum Type
        {
            Genesis
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Type BlockType { get; set; }

        public required Uri Image { get; set; }

        public required int Price { get; set; }

        //In Seconds
        public required int Duration { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public DateTime? Deleted { get; set; }

        public bool IsDeleted { get => Deleted != null; set => _ = value; }
    }
}