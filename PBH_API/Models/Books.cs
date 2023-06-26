namespace PBH_API.Models
{
    public class Books
    {
        public int Book_Id { get; set; }
        public string Title { get; set; }
        public int Category_Id { get; set; }
        public string Description { get; set; }
        public decimal Media_Rating { get; set; }
        public int Goal { get; set; }
        public int User_Id { get; set; }
        public int Institution_Id { get; set; }

        public UsersOut User { get; set; }
        public Institution Institution { get; set; }
        public BookCategories Categories { get; set; }
    }

}
