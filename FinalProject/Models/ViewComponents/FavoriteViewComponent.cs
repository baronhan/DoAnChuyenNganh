using FinalProject.Helpers;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Models.ViewComponents
{
    public class FavoriteViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var favorite = HttpContext.Session.Get<List<FavoriteListVM>>(MySetting.FAVORITE_KEY) ?? new List<FavoriteListVM>();
            int quantity = favorite.Sum(p => p.totalPost);

            return View("FavoritePanel", quantity);
        }
    }
}
