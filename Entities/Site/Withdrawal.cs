using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PEAS.Entities.Site
{
    public class Withdrawal
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Status
        {
            Pending,
            Succeeded,
            Failed
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Business Business { get; set; }

        public required long Amount { get; set; }

        public required Status WithdrawalStatus { get; set; }

        public required DateTime Created { get; set; }

        public DateTime? Completed { get; set; }
        
        [Timestamp]
        public required byte[] Version { get; set; }
    }
}