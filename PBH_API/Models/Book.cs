namespace PBH_API.Models
{
	public class Book
	{
		public int Book_Id { get; set; }
		public string Title { get; set; }
		public int Category { get; set; }
		public string Description { get; set; }
		public decimal Media_Rating { get; set; }
		public int Goal { get; set; }
		public int User_Id { get; set; }
		public int Institution_Id { get; set; }
		public string ImagePath { get; set; }
	}
}
