using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
	[Table("CartDetail")]
	public class CartDetails
	{
		public int id { get; set; }

		[Required]
		public int ShoppingCartId { get; set; }
		[Required]
		public int BookId { get; set; }
		[Required]
		public int Quantity { get; set; }
		public double Price { get; set; }
		public Book Book { get; set; }
		public ShoppingCart ShoppingCart { get; set; }
	}
}
