using PEAS.Entities.Site;

namespace PEAS.Entities.Booking
{
	public class Transaction
	{
		public int Id { get; set; }

		public required Business Business { get; set; }

		public required Order? Order { get; set; }

		public required string Title { get; set; }

		public required decimal Amount { get; set; }

		public required DateTime Created { get; set; }
	}
}