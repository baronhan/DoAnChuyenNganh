using FinalProject.Data;
using FinalProject.Models;
using FinalProject.Services;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RoomFeedbackService _roomFeedbackService;
        private readonly UserService _userService;

        private readonly QlptContext _db;

        public HomeController(ILogger<HomeController> logger, QlptContext context, RoomFeedbackService roomFeedbackService, UserService userService)
        {
            _logger = logger;
            _db = context;
            _roomFeedbackService = roomFeedbackService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "AdminHome");
            }
            _roomFeedbackService.UpdateExpiredPostsStatus();

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
