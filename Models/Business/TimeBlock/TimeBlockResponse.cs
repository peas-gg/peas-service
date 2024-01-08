using System;
namespace PEAS.Models.Business.TimeBlock
{
    public class TimeBlockResponse
    {
        public Guid Id { get; set; }

        public required string Title { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }
    }
}