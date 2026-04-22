using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
	[Table("Order")]
	public class Order
	{
		public int id { get; set; }
		[Required]
		public string UserId { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.Now;
		
		public int OrderStatusId { get; set; }
		public bool IsDeleted { get; set; } = false;
		[BindProperty]
		public string? Name { get; set; }
		[BindProperty]
		public string? Address { get; set; }
		[BindProperty]
		public string? PaymentMethod { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public List<OrderDetail> OrderDetails { get; set; }
	}
}
