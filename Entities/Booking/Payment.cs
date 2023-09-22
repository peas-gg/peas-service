﻿using Microsoft.EntityFrameworkCore;
using PEAS.Entities.Site;

namespace PEAS.Entities.Booking
{
    [Owned]
    public class Payment
    {
        public int Id { get; set; }

        public Currency Currency { get; set; }

        public required int Base { get; set; }

        public required int Deposit { get; set; }

        public required int Tip { get; set; }

        public required int Fee { get; set; }

        public required int Total { get; set; }

        public required DateTime Created { get; set; }
    }
}