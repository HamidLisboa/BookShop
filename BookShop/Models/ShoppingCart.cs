using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
	[Table("ShppingCart")]
	public class ShoppingCart
	{
		public int id { get; set; }
		[Required]
		public string UserId { get; set; }
		public bool isDeleted { get; set; } = false;
		public ICollection<CartDetails> CartDetails { get; set; }
	}
}