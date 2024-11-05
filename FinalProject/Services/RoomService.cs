using FinalProject.Data;
using FinalProject.ViewModels;
using Microsoft.EntityFrameworkCore;

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


        public int? AddNewRoomPost(RoomPostContentVM roomPostContentVM, int userId)
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
                    UserId = userId,
                    RoomTypeId = roomPostContentVM.RoomTypeId,
                    RoomName = $"{roomTypeName} {district}"
                };

                db.RoomPosts.Add(roomPost);
                db.SaveChanges();

                return roomPost.PostId; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
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

        public IEnumerable<RoomPostVM> GetRoomListByAddress(string address, int postId)
        {
            try
            {
                string district = ExtractDistrictFromAddress(address);

                if (string.IsNullOrEmpty(district))
                {
                    return Enumerable.Empty<RoomPostVM>();
                }
                else
                {
                    var roomList = (from rp in db.RoomPosts
                                    join ri in db.RoomImages on rp.PostId equals ri.PostId
                                    join rt in db.RoomTypes on rp.RoomTypeId equals rt.RoomTypeId
                                    join rs in db.RoomStatuses on rp.StatusId equals rs.RoomStatusId
                                    where rs.RoomStatusId == 1
                                          //&& ri.ImageTypeId == 1
                                          && !string.IsNullOrEmpty(rp.Address)
                                          && rp.PostId != postId // Ensure the postId is excluded
                                    select new
                                    {
                                        rp,
                                        ri,
                                        rt
                                    }).ToList() // Execute the query to get data in memory
                .Where(x => ExtractDistrictFromAddress(x.rp.Address) == district) // Now filter in memory
                .Select(x => new RoomPostVM
                {
                    PostId = x.rp.PostId,
                    RoomName = x.rp.RoomName,
                    Quantity = (int)x.rp.Quantity,
                    RoomDescription = x.rp.RoomDescription,
                    RoomImage = x.ri.ImageUrl,
                    RoomPrice = (decimal)x.rp.RoomPrice,
                    RoomSize = (decimal)x.rp.RoomSize,
                    RoomAddress = x.rp.Address.Replace(
                        x.rp.Address.Substring(
                            x.rp.Address.IndexOf(',') + 1,
                            x.rp.Address.IndexOf(',', x.rp.Address.IndexOf(',') + 1) - x.rp.Address.IndexOf(',')
                        ),
                        string.Empty),
                    RoomType = x.rt.TypeName
                }).ToList();

                    return roomList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<RoomPostVM>();
            }
        }

        public List<UtilityVM> GetUtilities()
        {
            try
            {
                var data = db.Utilities.Select(utility => new UtilityVM
                {
                    utilityId = utility.UtilityId,
                    utilityName = utility.UtilityName,
                    description = utility.Description,
                }).OrderBy(x => x.utilityName).ToList();

                return data;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<UtilityVM>();
            }
        }

        public void SaveSelectedUtilities(int postId, List<int> selectedUtilities)
        {
            try
            {
                foreach (var item in selectedUtilities)
                {
                    var data = new RoomUtility
                    {
                        PostId = postId,
                        UtilityId = item
                    };

                    db.RoomUtilities.Add(data);
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SaveRoomImage(int newRoomId, string uniqueFileName)
        {
            try
            {
                var roomImage = new RoomImage
                {
                    PostId = newRoomId,
                    ImageUrl = uniqueFileName
                };

                db.RoomImages.Add(roomImage);
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public IEnumerable<RoomManagementVM> GetRoomListByUserId(int userId)
        {
            try
            {
                var roomList = from r in db.RoomPosts
                               join img in db.RoomImages on r.PostId equals img.PostId into imgGroup
                               where r.UserId == userId
                               select new RoomManagementVM
                               {
                                   PostId = r.PostId,
                                   RoomPrice = (decimal)r.RoomPrice,
                                   RoomAddress = r.Address,
                                   RoomImage = imgGroup.FirstOrDefault().ImageUrl,
                                   RoomName = r.RoomName,
                                   RoomSize = (decimal)r.RoomSize
                               };
                return roomList;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<RoomManagementVM>();
            }
        }
    }
}
