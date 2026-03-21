using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
	[Table("OrderStatus")]
	public class OrderStatus
	{
		public int id { get; set; }
		[Required]
		[MaxLength(20)]
		public string ?StatusName { get; set; }
	}
}
