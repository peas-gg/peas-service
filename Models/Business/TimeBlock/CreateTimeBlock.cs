namespace PEAS.Models.Business.TimeBlock
{
    public class CreateTimeBlock
    {
        public required string Title { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }
    }
}