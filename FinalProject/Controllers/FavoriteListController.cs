using FinalProject.Data;
using FinalProject.Helpers;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    public class FavoriteListController : Controller
    {

        private readonly QlptContext db;

        public FavoriteListController(QlptContext db)
        {
            this.db = db;
        }


        public List<FavoriteListVM> FavoriteList => HttpContext.Session.Get<List<FavoriteListVM>>(MySetting.FAVORITE_KEY) ?? new List<FavoriteListVM>();
        public IActionResult Index()
        {
            return View(FavoriteList);
        }

        public IActionResult AddToFavorite(int id, int quantity = 1)
        {

            var favorite = HttpContext.Session.Get<List<FavoriteListVM>>(MySetting.FAVORITE_KEY) ?? new List<FavoriteListVM>();


            var item = favorite.SingleOrDefault(p => p.PostId == id);
            if (item == null)
            {

                var room = db.RoomPosts.SingleOrDefault(p => p.PostId == id);
                if (room == null)
                {
                    return View("_PageNotFound");
                }

                var result = db.RoomPosts
                    .Where(r => r.PostId == id)
                    .Join(
                        db.RoomImages,
                        r => r.PostId,
                        img => img.PostId,
                        (r, img) => new { r, img })
                    .Join(
                        db.ImageTypes,
                        ri => ri.img.ImageTypeId,
                        it => it.TypeId,
                        (ri, it) => new { ri.r, ri.img, it })
                    .Join(
                        db.RoomTypes,
                        rimg => rimg.r.RoomTypeId,
                        rt => rt.RoomTypeId,
                        (rimg, rt) => new { rimg.r, rimg.img, rimg.it, rt })
                    .Where(x => x.img.ImageTypeId == 1)
                    .Select(x => new FavoriteListVM
                    {
                        PostId = x.r.PostId,
                        RoomName = x.r.RoomName,
                        RoomImage = x.img.ImageUrl,
                        RoomPrice = (decimal)x.r.RoomPrice,
                        RoomSize = (decimal)x.r.RoomSize,
                        RoomAddress = x.r.Address,
                        totalPost = quantity
                    })
                    .FirstOrDefault();

                if (result != null)
                {
                    favorite.Add(result);
                }
            }

            HttpContext.Session.Set(MySetting.FAVORITE_KEY, favorite);

            return RedirectToAction("Index");
        }


    }
}
