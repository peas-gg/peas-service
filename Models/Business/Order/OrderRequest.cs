using PEAS.Helpers.Utilities;

namespace PEAS.Models.Business
{
    public class OrderRequest
    {
        public Guid BlockId { get; set; }
        public required DateTime Date { get; set; }
        public required DateRange DateRange { get; set; }
        public required string? FirstName { get; set; }
        public required string? Lastname { get; set; }
        public required string Email { get; set; }
        public required string? Phone { get; set; }
        public required string? Note { get; set; }
    }
}