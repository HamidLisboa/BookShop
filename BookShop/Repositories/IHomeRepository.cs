namespace BookShop.Repositories
{
	public interface IHomeRepository
	{
		Task<IEnumerable<Book>> GetBooks(string searchTerms = "", int genreId = 0);
		Task<IEnumerable<Genre>> Genres();
	}
}