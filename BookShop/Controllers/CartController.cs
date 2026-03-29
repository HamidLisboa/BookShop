using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
	public class CartController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
