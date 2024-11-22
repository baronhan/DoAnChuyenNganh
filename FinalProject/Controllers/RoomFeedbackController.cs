using FinalProject.Data;
using Microsoft.AspNetCore.Mvc;
using FinalProject.Services;
using System.Text.Json;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using FinalProject.Helpers;
using System.Diagnostics;

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

        public IActionResult SendResponse(int postId, int feedbackId)
        {
            SendResponseVM sendResponseVM = _roomFeedbackService.GetFeedbackInformation(postId, feedbackId);
            return View(sendResponseVM);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendResponse(SendResponseVM sendResponse)
        {
            if (ModelState.IsValid)
            {
                bool userHasResponded = _roomFeedbackService.CheckExistingResponse(sendResponse.PostId, sendResponse.FeedbackId);

                if (!userHasResponded)
                {
                    using (var transaction = _db.Database.BeginTransaction()) 
                    {
                        try
                        {
                            if (_roomFeedbackService.AddNewResponse(sendResponse))
                            {
                                int newResponseId = _roomFeedbackService.GetLastAddedResponseId();

                                // Kiểm tra và xử lý hình ảnh nếu có
                                if (sendResponse.UploadedImages != null && sendResponse.UploadedImages.Count > 0)
                                {
                                    string invalidObjects = await ProcessRoomImagesAsync(sendResponse.UploadedImages);

                                    if (!string.IsNullOrEmpty(invalidObjects))
                                    {
                                        // Nếu phát hiện ảnh không hợp lệ, rollback transaction và trả về thông báo
                                        TempData["FailMessage"] = invalidObjects;
                                        transaction.Rollback();
                                        return View(sendResponse);
                                    }

                                    // Nếu hình ảnh hợp lệ, lưu vào bảng ResponseImages
                                    SaveResponseImages(newResponseId, sendResponse.UploadedImages);
                                }

                                // Commit transaction sau khi xử lý thành công
                                transaction.Commit();
                                TempData["SuccessMessage"] = "Phản hồi đã được gửi thành công!";
                                return View(sendResponse);
                            }

                            else
                            {
                                TempData["FailMessage"] = "Đã xảy ra lỗi khi gửi phản hồi!";
                                transaction.Rollback();
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Transaction failed: {ex.Message}");
                            TempData["FailMessage"] = "Đã xảy ra lỗi trong quá trình xử lý!";
                        }
                    }
                }
                else
                {
                    TempData["FailMessage"] = "Bạn đã gửi phản hồi cho bài viết này rồi!";
                }
            }

            return View(sendResponse);
        }


        private async Task<string> ProcessRoomImagesAsync(List<IFormFile> roomImages)
        {
            foreach (var image in roomImages)
            {
                var rootPath = @"D:\HKI 2024-2025\SmartRoomManagement\FinalProject";

                var imagePath = Path.Combine(rootPath, "wwwroot", "img", "Temp", $"{DateTime.Now.Ticks}_{image.FileName}");

                try
                {
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    string invalidObjects = await DetectImageWithYOLO(imagePath);

                    if (invalidObjects != null)
                    {
                        var objects = invalidObjects.Split(',');
                        var translatedObjects = objects.Select(obj => TranslateObject.Translate(obj.Trim())).ToList();

                        return $"Ảnh chứa các đối tượng không phù hợp: {string.Join(", ", translatedObjects)}";
                    }
                }
                finally
                {
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }

            return null;
        }

        private async Task<string> DetectImageWithYOLO(string imagePath)
        {
            var result = await Task.Run(() =>
            {
                try
                {
                    string output = CallPythonScript(imagePath).Result;

                    Console.WriteLine($"Python output: {output}");

                    if (output.Contains("unsuitable objects"))
                    {
                        var unsuitableObjects = output.Split(new[] { "unsuitable objects:" }, StringSplitOptions.None)[1].Trim();
                        return unsuitableObjects;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in detection: {ex.Message}");
                    return "Error in detection";
                }
            });

            return result;
        }

        private async Task<string> CallPythonScript(string imagePath)
        {
            var pythonPath = @"D:\HKI 2024-2025\SmartRoomManagement\FinalProject\Scripts\yolov8_env\Scripts\python.exe";
            var scriptPath = @"D:\HKI 2024-2025\SmartRoomManagement\FinalProject\Scripts\yolo_detection.py";

            using (var process = new Process())
            {
                process.StartInfo.FileName = pythonPath;
                process.StartInfo.Arguments = $"\"{scriptPath}\" \"{imagePath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                return output;
            }
        }

        private void SaveResponseImages(int responseId, List<IFormFile>? uploadedImages)
        {
            if (uploadedImages != null && uploadedImages.Count > 0)
            {
                foreach (var image in uploadedImages)
                {
                    var originalFileName = image.FileName;
                    var fileExtension = Path.GetExtension(originalFileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

                    var filePath = Path.Combine("wwwroot/img/ResponseImage", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    _roomFeedbackService.SaveResponseImage(responseId, uniqueFileName);
                }
            }
        }

    }
}
