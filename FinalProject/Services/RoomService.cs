using FinalProject.Data;
using FinalProject.ViewModels;

namespace FinalProject.Services
{
    public class RoomService
    {
        private readonly QlptContext db;
        public RoomService(QlptContext db)
        {
            this.db = db;
        }

        public IEnumerable<RoomTypeMenuVM> GetRoomType()
        {
            try
            {
                var data = db.RoomTypes.Select(type => new RoomTypeMenuVM
                {
                    RoomTypeId = type.RoomTypeId,
                    TypeName = type.TypeName,
                }).OrderBy(x => x.TypeName).ToList();

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<RoomTypeMenuVM>(); ;
            }
        }

        public bool CheckExistingCoordinate(double latitude, double longitude)
        {
            try
            {
                var existingRoomCoordinate = db.RoomCoordinates.FirstOrDefault(coor => coor.Latitude == latitude && coor.Longitude == longitude);

                if (existingRoomCoordinate != null)
                {
                    return true;
                }

                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public int GetRoomCoordinateId(double latitude, double longitude)
        {
            try
            {
                var coordinate = db.RoomCoordinates.FirstOrDefault(coor => coor.Latitude == latitude && coor.Longitude == longitude);

                if(coordinate != null)
                {
                    return coordinate.RoomCoordinateId;
                }   
                else
                {
                    return 0;
                }    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        private string GetRoomTypeName(int roomTypeId)
        {
            try
            {
                string typeName = db.RoomTypes.Where(type => type.RoomTypeId == roomTypeId).Select(type => type.TypeName).SingleOrDefault(); 

                return typeName ?? string.Empty; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty; 
            }
        }

        private string ExtractDistrictFromAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return string.Empty;
            }

            var parts = address.Split(',');

            if (parts.Length >= 2)
            {
                var districtPart = parts[^1].Trim(); 

                if (districtPart.StartsWith("Quận", StringComparison.OrdinalIgnoreCase))
                {
                    return districtPart; 
                }
            }
            return string.Empty;
        }


        public bool AddNewRoomPost(RoomPostContentVM roomPostContentVM)
        {
            try
            {
                var roomTypeName = GetRoomTypeName(roomPostContentVM.RoomTypeId);
                if (string.IsNullOrEmpty(roomTypeName))
                {
                    throw new Exception("Room type name is null or empty.");
                }

                var district = ExtractDistrictFromAddress(roomPostContentVM.RoomAddress);
                if (string.IsNullOrEmpty(district))
                {
                    throw new Exception("District is null or empty.");
                }

                var roomPost = new RoomPost
                {
                    Quantity = roomPostContentVM.Quantity,
                    RoomCoordinateId = roomPostContentVM.RoomCoordinateId,
                    RoomPrice = roomPostContentVM.RoomPrice,
                    RoomSize = roomPostContentVM.RoomSize,
                    Address = roomPostContentVM.RoomAddress,
                    RoomDescription = roomPostContentVM.RoomDescription,
                    DatePosted = DateTime.Now,
                    ExpirationDate = DateTime.Now.AddDays(30),
                    StatusId = 1,
                    UserId = 2,
                    RoomTypeId = roomPostContentVM.RoomTypeId,
                    RoomName = $"{roomTypeName} {district}"
                };

                db.RoomPosts.Add(roomPost);

                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public int AddNewRoomCoordinate(double latitude, double longitude)
        {
            try
            {
                var roomCoordinate = new RoomCoordinates
                {
                    Latitude = latitude,
                    Longitude = longitude
                };

                db.RoomCoordinates.Add(roomCoordinate);

                db.SaveChanges();

                return roomCoordinate.RoomCoordinateId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}
