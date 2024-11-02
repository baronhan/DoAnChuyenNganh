using FinalProject.Data;
using FinalProject.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Services
{
    public class FavoriteListService
    {
        private readonly QlptContext db;

        public FavoriteListService(QlptContext db)
        {
            this.db = db;
        }

        public async Task<bool> CheckExistingFavoriteList(int user_id)
        {
            try
            {
                var existingFavoriteList = await db.FavoriteLists
                    .FirstOrDefaultAsync(favorite => favorite.UserId == user_id);

                return existingFavoriteList != null; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> AddFavotiteListPost(int id, int userId)
        {
            try
            {
                int favoriteId = await GetFavoriteIdByUserId(userId);

                if (favoriteId == 0)
                {
                    return false; 
                }

                var favoritePost = new FavoriteListPost
                {
                    PostId = id,
                    FavoriteId = favoriteId,
                };

                db.FavoriteListPosts.Add(favoritePost);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private async Task<int> GetFavoriteIdByUserId(int userId)
        {
            try
            {
                var favoriteId = await db.FavoriteLists
                    .Where(favorite => favorite.UserId == userId)
                    .Select(favorite => favorite.FavoriteListId)
                    .FirstOrDefaultAsync();

                return favoriteId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
        }

        public async Task<bool> AddFavoriteList(int userId)
        {
            try
            {
                var favoriteList = new FavoriteList
                {
                    UserId = userId,
                };

                db.FavoriteLists.Add(favoriteList);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public async Task<List<FavoriteListVM>> GetFavoriteListByUserId(int userId)
        {
            int favoriteId = await GetFavoriteIdByUserId(userId);

            var favorites = await (from f in db.FavoriteListPosts
                                   join r in db.RoomPosts on f.PostId equals r.PostId
                                   join img in db.RoomImages on r.PostId equals img.PostId
                                   join it in db.ImageTypes on img.ImageTypeId equals it.TypeId
                                   join rt in db.RoomTypes on r.RoomTypeId equals rt.RoomTypeId
                                   where f.FavoriteId == favoriteId && img.ImageTypeId == 1
                                   select new FavoriteListVM
                                   {
                                       PostId = r.PostId,
                                       RoomName = r.RoomName,
                                       RoomImage = img.ImageUrl,
                                       RoomPrice = (decimal)r.RoomPrice,
                                       RoomSize = (decimal)r.RoomSize,
                                       RoomAddress = r.Address
                                   }).ToListAsync(); 
            return favorites;
        }

    }
}