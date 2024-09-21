using FinalProject.Data;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    public class RoomPostController : Controller
    {
        private readonly QlptContext _db;

        public RoomPostController(QlptContext db)
        {
            _db = db;
        }


        public IActionResult Index(int? id)
        {
            var room = _db.RoomPostVM.FromSql($"GetRoomPosts {id}").ToList();
            return View(room);
        }


        [HttpGet]
        public IActionResult SearchRoom(int? roomType, string? district, string? ward, int? adult, string? priceRange)
        {
            List<RoomPostVM> rooms = new List<RoomPostVM>();

            using (var command = _db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC SearchRoom @RoomTypeId, @District, @Ward, @Adult, @PriceRange";

                command.Parameters.Add(new SqlParameter("@RoomTypeId", (object)roomType ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@District", (object)district ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@Ward", (object)ward ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@Adult", (object)adult ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@PriceRange", (object)priceRange ?? DBNull.Value));

                _db.Database.OpenConnection();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var room = new RoomPostVM
                        {
                            PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                            RoomName = reader.GetString(reader.GetOrdinal("RoomName")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            RoomDescription = reader.GetString(reader.GetOrdinal("RoomDescription")),
                            RoomImage = reader.GetString(reader.GetOrdinal("RoomImage")),
                            RoomPrice = reader.GetDecimal(reader.GetOrdinal("RoomPrice")),
                            RoomSize = reader.GetDecimal(reader.GetOrdinal("RoomSize")),
                            RoomAddress = reader.GetString(reader.GetOrdinal("RoomAddress")),
                            RoomType = reader.GetString(reader.GetOrdinal("RoomType"))
                        };

                        rooms.Add(room);
                    }
                }

                _db.Database.CloseConnection();
            }

            return View("SearchRoom", rooms);
        }



        public IActionResult Detail(int id)
        {
            var roomDetail = new RoomPostDetailVM();
            var roomImages = new List<RoomImageVM>();

            using (var command = _db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC GetRoomDetail @PostID";
                command.Parameters.Add(new SqlParameter("@PostID", id));

                _db.Database.OpenConnection();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        roomDetail = new RoomPostDetailVM
                        {
                            PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                            RoomName = reader.GetString(reader.GetOrdinal("RoomName")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            RoomDescription = reader.GetString(reader.GetOrdinal("RoomDescription")),
                            RoomPrice = reader.GetDecimal(reader.GetOrdinal("RoomPrice")),
                            RoomSize = reader.GetDecimal(reader.GetOrdinal("RoomSize")),
                            RoomAddress = reader.GetString(reader.GetOrdinal("RoomAddress")),
                            RoomType = reader.GetString(reader.GetOrdinal("RoomType")),
                            DatePosted = reader.GetDateTime(reader.GetOrdinal("DatePosted")),
                            ExpirationDate = reader.GetDateTime(reader.GetOrdinal("ExpirationDate")),
                            FullName = reader.GetString(reader.GetOrdinal("FullName")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Gender = reader.GetString(reader.GetOrdinal("Gender")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            UtilityNames = reader.GetString(reader.GetOrdinal("UtilityNames")),
                            UtilityDescriptions = reader.GetString(reader.GetOrdinal("UtilityDescriptions"))
                        };
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("RoomImage")))
                            {
                                roomImages.Add(new RoomImageVM
                                {
                                    RoomImage = reader.GetString(reader.GetOrdinal("RoomImage"))
                                });
                            }
                        }
                    }
                }

                _db.Database.CloseConnection();
            }

            var model = new RoomDetailAndImagesVM
            {
                RoomDetail = roomDetail,
                RoomImages = roomImages
            };

            return View(model);
        }



    }
}
