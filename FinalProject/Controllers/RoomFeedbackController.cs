using FinalProject.Data;
using Microsoft.AspNetCore.Mvc;
using FinalProject.Services;
using System.Text.Json;

namespace FinalProject.Controllers
{
    public class RoomFeedbackController : Controller
    {
        private readonly QlptContext _db;
        private readonly RoomFeedbackService _roomFeedbackService;

        public RoomFeedbackController(QlptContext db, RoomFeedbackService roomFeedbackService)
        {
            _db = db;
            _roomFeedbackService = roomFeedbackService;
        }
        public IActionResult Index(int postID)
        {
            ViewBag.PostID = postID;
            ViewBag.Feedbacks = _db.Feedbacks.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Report(int postID, int selectedFeedbackType)
        {
            var userID = Request.Cookies["user_id"];
            string s = await _roomFeedbackService.SendReport(Int32.Parse(userID), postID, selectedFeedbackType);
            TempData["SuccessMessage"] = TempData["SuccessMessage"] = JsonSerializer.Serialize(s); ;
            return RedirectToAction("Detail", "RoomPost", new { id = postID });
        }

        public IActionResult Response(int postId, int feedbackId)
        {
            return View();
        }

    }
}
