namespace PBH_API.Models
{
	public class Rating
	{

		public int rating { get; set; }
		public String Comment { get; set; }

		public int Book_Id { get; set; }
		public int User_Id { get; set; }
	}
}
