using System;
namespace PEAS.Models.Business
{
	public class OrderRequest
	{
		public Guid BlockId { get; set; }
		public DateTime Date { get; set; }
		public required string FirstName { get; set; }
		public required string Lastname { get; set; }
		public required string Email { get; set; }
		public required string Phone { get; set; }
	}
}