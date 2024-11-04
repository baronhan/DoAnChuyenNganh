using FinalProject.Data;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using FinalProject.Services;
using Newtonsoft.Json.Linq;

namespace FinalProject.Controllers
{
    public class RoomPostController : Controller
    {
        private readonly QlptContext _db;
        private readonly RoomService _roomService;
        private readonly IHttpClientFactory _httpClientFactory;

        public RoomPostController(QlptContext db, RoomService _roomService, IHttpClientFactory _httpClientFactory)
        {
            this._db = db;
            this._roomService = _roomService;
            this._httpClientFactory = _httpClientFactory;
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
                            Gender = !reader.IsDBNull(reader.GetOrdinal("Gender")) && reader.GetBoolean(reader.GetOrdinal("Gender")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            UtilityNames = reader.GetString(reader.GetOrdinal("UtilityNames")),
                            UtilityDescriptions = reader.GetString(reader.GetOrdinal("UtilityDescriptions")),
                            Latitude = reader.GetDouble(reader.GetOrdinal("Latitude")),
                            Longitude = reader.GetDouble(reader.GetOrdinal("Longitude")),
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

        [Authorize]
        public IActionResult UploadRoomPost()
        {
            var model = new UploadRoomPostVM
            {
                RoomTypes = _roomService.GetRoomType(),
                RoomPostContentVM = new RoomPostContentVM()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadRoomPost(UploadRoomPostVM model)
        {
            ModelState.Remove("RoomTypes");

            if (ModelState.IsValid)
            {
                var coordinates = await GetCoordinatesAsync(model.RoomPostContentVM.RoomAddress);

                if (coordinates.latitude == 0 && coordinates.longitude == 0)
                {
                    TempData["FailMessage"] = "Invalid coordinates.";
                    model.RoomTypes = _roomService.GetRoomType(); 
                    return View(model);
                }

                if (_roomService.CheckExistingCoordinate(coordinates.latitude, coordinates.longitude))
                {
                    var roomCoordinateId = _roomService.GetRoomCoordinateId(coordinates.latitude, coordinates.longitude);

                    if (roomCoordinateId != 0)
                    {
                        model.RoomPostContentVM.RoomCoordinateId = roomCoordinateId;

                        if (_roomService.AddNewRoomPost(model.RoomPostContentVM))
                        {
                            TempData["SuccessMessage"] = "Added to favorites successfully!";
                        }
                        else
                        {
                            TempData["FailMessage"] = "Failed to add to favorites!";
                        }
                    }
                    else
                    {
                        TempData["FailMessage"] = "Room Coordinate Id doesn't exist!";
                    }
                }
                else
                {
                    int roomCoordinateId = _roomService.AddNewRoomCoordinate(coordinates.latitude, coordinates.longitude);

                    if (roomCoordinateId != 0)
                    {
                        model.RoomPostContentVM.RoomCoordinateId = roomCoordinateId;

                        if (_roomService.AddNewRoomPost(model.RoomPostContentVM))
                        {
                            TempData["SuccessMessage"] = "Added to favorites successfully!";
                        }
                        else
                        {
                            TempData["FailMessage"] = "Failed to add to favorites!";
                        }
                    }
                    else
                    {
                        TempData["FailMessage"] = "Failed to add new Room Coordinate!";
                    }
                }

                model.RoomTypes = _roomService.GetRoomType();

                return View(model);
            }
            else
            {
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"Property: {modelState.Key}, Error: {error.ErrorMessage}");
                    }
                }

                model.RoomTypes = _roomService.GetRoomType(); 
                return View(model);
            }
        }



        private async Task<(double latitude, double longitude)> GetCoordinatesAsync(string address)
        {
            var client = _httpClientFactory.CreateClient();
            string url = $"https://localhost:7127/api/geocode/getCoordinates?address={Uri.EscapeDataString(address)}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Geocode API Response: {jsonResponse}");

                var result = JObject.Parse(jsonResponse);

                if (result["latitude"] != null && result["longitude"] != null)
                {
                    double latitude = result["latitude"].Value<double>();
                    double longitude = result["longitude"].Value<double>();
                    return (latitude, longitude);
                }
                else
                {
                    Console.WriteLine($"Geocoding error: {result["status"]}"); 
                }
            }
            else
            {
                Console.WriteLine($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
            }


            System.Diagnostics.Debug.WriteLine(response);

            return (0, 0); 
        }



    }
}
