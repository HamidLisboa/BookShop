using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
	[Table("Book")]
	public class Book
	{
		public int id { get; set; }
		[Required]
		[MaxLength(40)]
		public string? BookName{ get; set; }
		[Required]
		public double Price{ get; set; }
		[Required]
		public int GenreId{ get; set; }
		public string? Image { get; set; }
		public Genre Genre { get; set; }
		public List<OrderDetail> OrderDetails { get; set; }
		public List<CartDetails> CartDetails { get; set; }
	}
}
