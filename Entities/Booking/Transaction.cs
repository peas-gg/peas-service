using System.Text.Json.Serialization;
using PEAS.Entities.Site;

namespace PEAS.Entities.Booking
{
    public class Transaction
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum Type
        {
            Deposit,
            Withdrawal
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
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