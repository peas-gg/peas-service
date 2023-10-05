using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PEAS.Entities.Booking;

namespace PEAS.Entities.Site
{
    public class Transaction
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Type
        {
            Deposit,
            Withdrawal
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Status
        {
            Processing,
            Succeeded,
            Failed
        }

        public int Id { get; set; }

        public required Business Business { get; set; }

        public required Type TransactionType { get; set; }

        public required Status TransactionStatus { get; set; }

        public required Order? Order { get; set; }

        public required string Title { get; set; }

        public required int Amount { get; set; }

        public required DateTime Created { get; set; }
    }
}