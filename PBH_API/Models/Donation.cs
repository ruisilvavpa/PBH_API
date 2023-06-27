namespace PBH_API.Models
{
	public class Donation
	{
		public int DonationId { get; set; }
		public int BookId { get; set; }
		public int UserId { get; set; }
		public decimal DonationAmount { get; set; }
	}
}
