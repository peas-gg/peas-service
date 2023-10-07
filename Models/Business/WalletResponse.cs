using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PEAS.Models.Business.Order;

namespace PEAS.Models.Business
{
    public class WalletResponse
    {
        public class TransactionResponse
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Type
            {
                Earning,
                Withdrawal
            }

            public Type TransactionType { get; set; }

            public EarningResponse? Earning { get; set; }

            public WithdrawalResponse? Withdrawal { get; set; }
        }

        public long Balance { get; set; }

        public long HoldBalance { get; set; }

        public required List<TransactionResponse> Transactions { get; set; }
    }
}