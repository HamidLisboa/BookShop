using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace BookShop.Controllers
{
	public class CartController : Controller
	{
		private readonly ICartRepository _cartRepository;

		public CartController(ICartRepository cartRepository)
		{
			_cartRepository = cartRepository;
		}
		public async Task<IActionResult> AddItemtoCart(int bookId, int quantity = 1, int redirect = 0)
		{

			var cartCount = await _cartRepository.AddItemToCart(bookId, quantity);
			if (redirect == 0)
				return Ok(cartCount);
			return RedirectToAction("GetUserCart");
		}
		public async Task<IActionResult> RemoveItemfromCart(int bookId)
		{
			var cartCount = await _cartRepository.RemoveItemFromCart(bookId);
			return RedirectToAction("GetUserCart");
		}
		public async Task<IActionResult> GetUserCart()
		{
			var cart = await _cartRepository.GetUserCart();
			return View(cart);
		}
		public async Task<IActionResult> GetTotalItems()
		{
			int cartItems = await _cartRepository.GetCartItemsCount();
			return Ok(cartItems);
		}
      public async Task<IActionResult> Checkout()
		{
			var orderDetail = await _cartRepository.Checkout(); 
			if (orderDetail == null)
				return NotFound();
			return View(model: orderDetail);
		}
		[HttpPost]
		public async Task<IActionResult> ShippingDatas(string address, string name, string paymentMethod)
		{
			var order = await _cartRepository.ShippingDatas(address, name, paymentMethod);
			if (order == null)
				return NotFound();
			return View(model: order);
		}
		 public async Task<IActionResult> OrderConfirmation(int orderId)
		{
			var isConfirmed = await _cartRepository.OrderConfirmation(orderId);
			if (!isConfirmed)
				return BadRequest("Order confirmation failed.");
			return View();
		}
	}
}
