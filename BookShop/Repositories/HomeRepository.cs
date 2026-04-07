using Microsoft.EntityFrameworkCore;

namespace BookShop.Repositories
{
	public class HomeRepository : IHomeRepository
	{
		private readonly ApplicationDbContext _context;
		public HomeRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task<IEnumerable<Genre>> Genres()
		{
			return await _context.Genres.ToListAsync();
		}
		public async Task<IEnumerable<Book>> GetBooks(string searchTerm = "", int genreId = 0)
		{
			var bookQuery = _context.Books
			   .AsNoTracking()
			   .Include(x => x.Genre)
			   //.Include(x => x.Stock)
			   .AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				bookQuery = bookQuery.Where(b => b.BookName.Contains(searchTerm.ToLower()));
			}

			if (genreId > 0)
			{
				bookQuery = bookQuery.Where(b => b.GenreId == genreId);
			}

			var books = await bookQuery
				.AsNoTracking()
				.Select(book => new Book
				{
					id = book.id,
					Image = book.Image,
					AuthorName = book.AuthorName,
					BookName = book.BookName,
					GenreId = book.GenreId,
					Price = book.Price,
					GenreName = book.Genre.GenreName,
					//Quantity = book.Stock == null ? 0 : book.Stock.Quantity
				}).ToListAsync();
			return books;
		}
	}
}
