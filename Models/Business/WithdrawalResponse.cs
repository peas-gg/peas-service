using static PEAS.Entities.Site.Withdrawal;

namespace PEAS.Models.Business.Order
{
    public class WithdrawalResponse
    {
        public required int Amount { get; set; }

        public required Status WithdrawalStatus { get; set; }

        public required DateTime Created { get; set; }

        public DateTime? Completed { get; set; }
    }
}