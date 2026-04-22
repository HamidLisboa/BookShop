using BookShop.Data.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Repositories
{
	public class CartRepository : ICartRepository
	{
		private readonly ApplicationDbContext _dbcontext;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<IdentityUser> _userManager;

		public CartRepository(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor
								, UserManager<IdentityUser> userManager)
		{
			_dbcontext = dbContext;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
		}
		public async Task<int> AddItemToCart(int bookId, int quantity = 1)
		{
			string userId = GetUserId();
			using var transaction = await _dbcontext.Database.BeginTransactionAsync();
			try
			{
				if (string.IsNullOrEmpty(userId))
					throw new UnauthorizedAccessException("User is not authenticated.");
				var cart = await GetCart(userId);
				if (cart is null)
				{
					cart = new ShoppingCart
					{
						UserId = userId
					};
					_dbcontext.ShoppingCarts.Add(cart);
				}
				_dbcontext.SaveChanges();
				var cartItem = await _dbcontext.CartDetails.FirstOrDefaultAsync(x => x.BookId == bookId && x.ShoppingCartId == cart.id);
				if (cartItem is not null)
				{
					cartItem.Quantity += quantity;
				}
				else
				{
					var book = await _dbcontext.Books.FindAsync(bookId);
					cartItem = new CartDetails
					{
						BookId = bookId,
						ShoppingCartId = cart.id,
						Quantity = quantity,
						Price = book.Price
					};
					_dbcontext.CartDetails.Add(cartItem);
				}
				_dbcontext.SaveChanges();
				transaction.Commit();
			}

			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
			// return the updated cart items count for the user
			var itemCartCount = await GetCartItemsCount(userId);
			return itemCartCount;

		}
		public async Task<int> RemoveItemFromCart(int bookId)
		{
			string userId = GetUserId();

			try
			{
				if (string.IsNullOrEmpty(userId))
					throw new UnauthorizedAccessException("User is not authenticated.");

				var cartItem = await _dbcontext.CartDetails
					.FirstOrDefaultAsync(x => x.BookId == bookId
										   && x.ShoppingCart.UserId == userId);

				if (cartItem is null)
					throw new InvalidOperationException("Book not found in the shopping cart.");

				if (cartItem.Quantity == 1)
					_dbcontext.CartDetails.Remove(cartItem);
				else
					cartItem.Quantity--;

				await _dbcontext.SaveChangesAsync();
			}

			catch
			{

			}
			var itemCartCount = await GetCartItemsCount(userId);
			return itemCartCount;

		}
		public async Task<ShoppingCart> GetUserCart()
		{
			string userId = GetUserId();
			if (userId is null)
				throw new InvalidOperationException("Unable to find user!");
			var shoppingCart = await _dbcontext.ShoppingCarts.
									Include(x => x.CartDetails)
									.ThenInclude(x => x.Book)
									.ThenInclude(x => x.Stock)
									.Include(x => x.CartDetails)
									.ThenInclude(x => x.Book)
									.ThenInclude(x => x.Genre)
									.Where(x => x.UserId == userId).FirstOrDefaultAsync();
			return shoppingCart;
		}
		public async Task<ShoppingCart> GetCart(string userId)
		{
			var cart = await _dbcontext.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
			return cart;
		}
		public async Task<int> GetCartItemsCount(string userId)
		{
			if (userId is null)
				userId = GetUserId();
			var count = await _dbcontext.CartDetails
			.Where(cd => cd.ShoppingCart.UserId == userId)
			.CountAsync();
			return count;
		}
		private string GetUserId()
		{
			var user = _httpContextAccessor.HttpContext.User;
			if (user is null)
				return null;
			string userId = _userManager.GetUserId(user);
			return userId;
		}

		public async Task<Order> Checkout()
		{
			using var transaction = await _dbcontext.Database.BeginTransactionAsync();
			try
			{
				string userId = GetUserId();
			if (string.IsNullOrEmpty(userId))
				throw new UnauthorizedAccessException("User is not authenticated.");
			var cart = await GetUserCart();
			var count = await GetCartItemsCount(userId);
			if (cart is null || count == 0)
				throw new InvalidOperationException("Shopping cart is empty.");
			var pendingRecord = _dbcontext.orderStatuses.FirstOrDefault(s => s.StatusName == "pending");
				var order = new Order
				{
					UserId = userId,
					CreatedDate = DateTime.UtcNow,
					OrderStatusId = pendingRecord.id,
					Address = null,
					Name = null,
					PaymentMethod = null
				};
			await _dbcontext.Orders.AddAsync(order);
			await _dbcontext.SaveChangesAsync();
			foreach (var item in cart.CartDetails)
			{
				var orderDetail = new OrderDetail
				{
					OrderId = order.id,
					BookId = item.BookId,
					Quantity = item.Quantity,
					UnitPrice = item.Price
				};
					
				await _dbcontext.OrderDetails.AddAsync(orderDetail);
					await _dbcontext.SaveChangesAsync();
					
			}
			transaction.Commit();
			var orderDetailResults = await _dbcontext.Orders
				.Include(o => o.OrderDetails)
				.ThenInclude(od => od.Book)
				.Where(o => o.id == order.id)
				.FirstOrDefaultAsync();
			return orderDetailResults;
			}
			catch
			{
				await transaction.RollbackAsync();
				return null;
			}
		}
		public async Task<Order> ShippingDatas(string address, string name, string paymentMethod)
		{
			await using var transaction = await _dbcontext.Database.BeginTransactionAsync();
			try
			{
				string userId = GetUserId();
				if (string.IsNullOrEmpty(userId))
					throw new UnauthorizedAccessException("User is not authenticated.");

				var cart = await GetUserCart();
				var count = await GetCartItemsCount(userId);
				if (cart is null || count == 0)
					throw new InvalidOperationException("Shopping cart is empty.");

				var order = await _dbcontext.Orders
					.Where(o => o.UserId == userId)
					.OrderByDescending(o => o.CreatedDate)
					.FirstOrDefaultAsync();

				if (order == null)
					throw new InvalidOperationException("Order not found.");

				// materialize with async
				

				order.Address = address;
				order.Name = name;
				order.PaymentMethod = paymentMethod;
				
				

				_dbcontext.Orders.Update(order);
				await _dbcontext.SaveChangesAsync();

				await transaction.CommitAsync();
				return order;
			}
			catch
			{
				await transaction.RollbackAsync();
				return null;
			}
		}
		public async Task<bool> OrderConfirmation(int id)
		{
			using var transaction = await _dbcontext.Database.BeginTransactionAsync();
			try
			{
				string userId = GetUserId();
			if (string.IsNullOrEmpty(userId))
				throw new UnauthorizedAccessException("User is not authenticated.");
			var cart = await GetUserCart();
			var count = await GetCartItemsCount(userId);
			if (cart is null || count == 0)
				throw new InvalidOperationException("Shopping cart is empty.");
			var confirmedRecord = _dbcontext.orderStatuses.FirstOrDefault(s => s.StatusName == "confirmed");
				var order = new Order
				{
					UserId = userId,
					OrderStatusId = confirmedRecord.id,
				};
			_dbcontext.Orders.Update(order);
			_dbcontext.SaveChanges();
			_dbcontext.CartDetails.RemoveRange(cart.CartDetails);
			await _dbcontext.SaveChangesAsync();
			transaction.Commit();
			return true;
			}
			catch
			{
				await transaction.RollbackAsync();
				return false;
			}
		}

	}
}

