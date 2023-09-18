using System.ComponentModel.DataAnnotations;

namespace PEAS.Entities.Site
{
    public class Wallet
    {
        public required Business Business { get; set; }

        public required int Balance { get; set; }

        public required int PendingDeposit { get; set; }

        public required int PendingWithdrawal { get; set; }

        [Timestamp]
        public required byte[] Version { get; set; }
    }
}