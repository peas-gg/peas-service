using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PEAS.Entities.Site
{
    [Owned]
    public class Block
    {
        [JsonConverter(typeof(StringEnumConverter))]
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

        public required bool IsActive { get; set; }

        public required DateTime Created { get; set; }
    }
}