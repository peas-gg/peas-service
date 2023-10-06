using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PEAS.Models.Business.Order;

namespace PEAS.Models.Business
{
    public class WalletResponse
    {
        public class Transaction
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Type
            {
                Order,
                Withdrawal
            }

            public Type TransationType { get; set; }

            public OrderResponse? Order { get; set; }

            public WithdrawalResponse? Withdrawal { get; set; }
        }

        public long Balance { get; set; }

        public long HoldBalance { get; set; }

        public required List<Transaction> Transactions { get; set; }
    }
}