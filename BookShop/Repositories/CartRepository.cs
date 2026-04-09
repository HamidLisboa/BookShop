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

		public async Task Checkout()
		{
			string userId = GetUserId();
			if (string.IsNullOrEmpty(userId))
				throw new UnauthorizedAccessException("User is not authenticated.");
			var cart = await GetCart(userId);
			if (cart is null || cart.CartDetails.Count == 0)
				throw new InvalidOperationException("Shopping cart is empty.");
			var order = new Order
			{
				UserId = userId,
				CreatedDate = DateTime.UtcNow,

				OrderDetails = cart.CartDetails.Select(cd => new OrderDetail
				{
					BookId = cd.BookId,
					Quantity = cd.Quantity,
					UnitPrice = cd.Price
				}).ToList()
			};
			using var transaction = await _dbcontext.Database.BeginTransactionAsync();
			try
			{
				_dbcontext.Orders.Add(order);
				_dbcontext.CartDetails.RemoveRange(cart.CartDetails);
				await _dbcontext.SaveChangesAsync();
				transaction.Commit();
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}
	}
}

