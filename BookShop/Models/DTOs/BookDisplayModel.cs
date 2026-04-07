namespace BookShop.Models.DTOs
{
	public class BookDisplayModel
	{
		public IEnumerable<Book> Books { get; set; }
		public IEnumerable<Genre> Genres { get; set; }

		public string searchTerm { get; set; } = string.Empty;
		public int genreId { get; set; } = 0;
	}
}
