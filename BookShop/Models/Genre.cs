using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
	[Table("Genre")]
	public class Genre
	{
		public int id { get; set; }
		public string GenreName { get; set; }
		public List<Book> Books { get; set; }

	}
}
