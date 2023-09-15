﻿using System.ComponentModel.DataAnnotations;

namespace PEAS.Entities.Site
{
    public class Wallet
    {
        public required Business Business { get; set; }

        public required decimal Balance { get; set; }

        public required decimal PendingDeposit { get; set; }

        public required decimal PendingWithdrawal { get; set; }

        [Timestamp]
        public required byte[] Version { get; set; }
    }
}