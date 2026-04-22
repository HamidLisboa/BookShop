using Microsoft.AspNetCore.Mvc;

namespace BookShop.Repositories
{
	public interface ICartRepository
	{
		Task<int> AddItemToCart(int bookId, int quantity);
		Task<int> RemoveItemFromCart(int bookId);
		Task<ShoppingCart> GetUserCart();
		Task<ShoppingCart> GetCart(string userId);
		Task<int> GetCartItemsCount(string userId = "");
		Task<Order> Checkout();
		Task<Order> ShippingDatas(string address, string name, string paymentMethod);
		Task<bool> OrderConfirmation(int orderId);
		
	}
}