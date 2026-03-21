using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
	[Table("Order")]
	public class Order
	{
		public int id { get; set; }
		[Required]
		public int UserId { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.Now;
		[Required]
		public int OrderId { get; set; }
		public bool IsDeleted { get; set; } = false;
		public OrderStatus OrderStatus { get; set; }
		public List<OrderDetail> OrderDetails { get; set; }
	}
}
