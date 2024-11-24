using FinalProject.Data;
using FinalProject.Services;
using FinalProject.Services.Admin;
using FinalProject.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;

namespace FinalProject.Controllers.Admin
{
    public class ViolationResponseManagementController : Controller
    {
        private readonly QlptContext _db;
        private readonly ResponseListService responseListService;
        private readonly RoomService _roomService;
        public ViolationResponseManagementController(QlptContext db,ResponseListService responseListService, RoomService roomService) 
        {
            this.responseListService = responseListService;
            this._roomService = roomService;
            this._db = db;
        }

        [Authorize]
        public IActionResult Index()
        {
            List<ResponseListVM> responseList = responseListService.GetAllResponseList();

            return View(responseList);
        }

        [Authorize]
        public IActionResult ViewResponse(int responseId)
        {
            ResponseListVM response = responseListService.GetResponseById(responseId);

            return View(response);
        }

        [Authorize]
        public IActionResult Approve(int responseId)
        {
            if (responseListService.Approve(responseId))
            {
                TempData["SuccessMessage"] = "Phản hồi đã được duyệt thành công!";
                return RedirectToAction("Index", "ViolationResponseManagement"); 
            }
            else
            {
                TempData["FailMessage"] = "Duyệt phản hồi thất bại!";
                return RedirectToAction("Index", "ViolationResponseManagement"); 
            }
        }

        [Authorize]
        public IActionResult Reject(int responseId)
        {
            try
            {
                if (responseListService.Reject(responseId))
                {
                    TempData["SuccessMessage"] = "Phản hồi đã bị từ chối thành công!";
                }
                else
                {
                    TempData["FailMessage"] = "Từ chối phản hồi thất bại!";
                }

                return RedirectToAction("Index", "ViolationResponseManagement");
            }
            catch (Exception ex)
            {
                TempData["FailMessage"] = "Đã xảy ra lỗi khi từ chối phản hồi.";
                return RedirectToAction("Index", "ViolationResponseManagement");
            }
        }

        public IActionResult ViolatingPostManagement(int pageNumber = 1, int pageSize = 7, string searchQuery = "")
        {
            int totalItems;
            List<RoomPostManagementVM> violatingPost;

            if (string.IsNullOrEmpty(searchQuery))
            {
                totalItems = responseListService.GetViolatingPostCount();
                violatingPost = responseListService.GetViolatingPost(pageNumber, pageSize);
            }
            else
            {
                var searchResults = responseListService.SearchViolatingPost(searchQuery);
                totalItems = searchResults.Count;
                violatingPost = searchResults
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            var paginationModel = new PagedListVM<RoomPostManagementVM>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = violatingPost
            };

            ViewBag.SearchQuery = searchQuery;
            return View("ViolatingPostManagement", paginationModel);
        }

        public async Task<IActionResult> DeleteViolatingPost(int postId)
        {
            var roomPost = await _roomService.GetRoomPostById(postId);
            if (roomPost == null)
            {
                TempData["FailMessage"] = "Phòng không tồn tại!";
                return RedirectToAction("ViolatingPostManagement", "ViolationResponseManagement");
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (await _roomService.HasRoomUtilities(postId))
                    {
                        await _roomService.DeleteRoomUtilities(postId);
                    }

                    if (await _roomService.HasRoomFavorites(postId))
                    {
                        await _roomService.DeleteRoomFavorites(postId);
                    }

                    if (await _roomService.HasRoomImages(postId))
                    {
                        await _roomService.DeleteRoomImages(postId);
                    }

                    if (await _roomService.HasRoomFeedbacks(postId))
                    {
                        await _roomService.DeleteRoomFeedbacks(postId);
                    }

                    if (await _roomService.HasBillService(postId))
                    {
                        await _roomService.DeleteBillService(postId);
                    }

                    await _roomService.DeleteRoomPost(postId);

                    int roomCoordinateId = roomPost.RoomCoordinateId;
                    if (!await _roomService.HasOtherPostsWithCoordinate(roomCoordinateId))
                    {
                        await _roomService.DeleteRoomCoordinateByIdAsync(roomCoordinateId);
                    }

                    transaction.Commit();

                    TempData["SuccessMessage"] = "Xóa phòng thành công!";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["FailMessage"] = "Đã có lỗi xảy ra khi xóa phòng. Vui lòng thử lại!";
                }
            }

            return RedirectToAction("ViolatingPostManagement", "ViolationResponseManagement");
        }
    }
}
