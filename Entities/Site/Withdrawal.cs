using System.ComponentModel.DataAnnotations;
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

        public required Business Business { get; set; }

        public required int Amount { get; set; }

        public required Status WithdrawalStatus { get; set; }

        public required DateTime Created { get; set; }

        public required DateTime? Completed { get; set; }
        
        [Timestamp]
        public required byte[] Version { get; set; }
    }
}