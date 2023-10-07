using System;
namespace PEAS.Models.Business
{
    public class EarningResponse
    {
        public required Guid OrderId { get; set; }

        public required string Title { get; set; }

        public required int Base { get; set; }

        public required int Deposit { get; set; }

        public required int Tip { get; set; }

        public required int Fee { get; set; }

        public required int Total { get; set; }

        public required DateTime Created { get; set; }

        public DateTime? Completed { get; set; }
    }
}