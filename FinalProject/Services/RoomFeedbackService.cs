using FinalProject.Data;
using FinalProject.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace FinalProject.Services
{
    public class RoomFeedbackService
    {
        private readonly QlptContext db;
        public RoomFeedbackService(QlptContext db)
        {
            this.db = db;
        }
        public List<FeedbackResultVM> NotifyUserForViolationPosts(int userId)
        {
            List<FeedbackResultVM> ds = new List<FeedbackResultVM>();
            var posts = db.RoomPosts.Where(t => t.UserId == userId).ToList();
            if (posts != null)
            {

                foreach (var post in posts)
                {
                    var postsViolation = db.RoomFeedbacks.Where(t => t.PostId == post.PostId)
                        .GroupBy(t => new { t.PostId, t.FeedbackId })
                        .Select(g => new
                        {
                            PostId = g.Key.PostId,
                            FeedbackId = g.Key.FeedbackId,
                            ViolationCount = g.Count()
                        }).ToList();
                    foreach (var item in postsViolation)
                    {
                        if (item.ViolationCount >= 5 && item.ViolationCount < 15)
                        {
                            var p = db.RoomPosts.FirstOrDefault(t => t.PostId == item.PostId);
                            var fb = db.Feedbacks.FirstOrDefault(t => t.FeedbackId == item.FeedbackId);
                            if (p != null && fb != null)
                            {
                                FeedbackResultVM feedbackResultVM = new FeedbackResultVM { ViolationCount = item.ViolationCount, Address = p.Address, FeedbackName = fb.FeedbackName };
                                ds.Add(feedbackResultVM);
                            }
                        }
                        else if (item.ViolationCount >= 15 && item.ViolationCount < 50)
                        {
                            var p = db.RoomPosts.FirstOrDefault(t => t.PostId == item.PostId);
                            var fb = db.Feedbacks.FirstOrDefault(t => t.FeedbackId == item.FeedbackId);
                            if (p != null && fb != null)
                            {
                                FeedbackResultVM feedbackResultVM = new FeedbackResultVM { ViolationCount = item.ViolationCount, Address = p.Address, FeedbackName = fb.FeedbackName };
                                ds.Add(feedbackResultVM);
                            }
                            HidePost(item.PostId);
                        }
                    }
                }
            }

            return ds;
        }

        private void HidePost(int? postId)
        {
            var post = db.RoomPosts.FirstOrDefault(p => p.PostId == postId);
            if (post != null)
            {
                post.StatusId = 4;
                db.SaveChanges();
            }
        }

        public void LockAccount(int? userId)
        {
            var hasViolation = db.RoomFeedbacks
                .Join(db.RoomPosts,
                      rf => rf.PostId,
                      rp => rp.PostId,
                      (rf, rp) => new { rf, rp })
                .Where(joined => joined.rp.UserId == userId)
                .GroupBy(joined => joined.rf.FeedbackId)
                .Any(g => g.Count() > 50);

            if (hasViolation)
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.IsValid = false;
                    db.SaveChanges(); 
                }
            }
        }

        public async Task<string> SendReport(int userID, int postID, int feedbackID)
        {
            try
            {
                var existingReport = await db.RoomFeedbacks
                    .FirstOrDefaultAsync(f => f.UserId == userID && f.PostId == postID);

                if (existingReport != null)
                {
                    return "Bạn chỉ có thể báo cáo bài đăng này một lần.";
                }

                var newRoomFeedback = new RoomFeedback
                {
                    UserId = userID,
                    PostId = postID,
                    FeedbackId = feedbackID,
                    Date = DateTime.Now
                };

                db.RoomFeedbacks.Add(newRoomFeedback);
                await db.SaveChangesAsync();

                var post = db.RoomPosts.FirstOrDefault(t => t.PostId == postID);
                if (post != null)
                {
                    LockAccount(post.UserId);
                }
                return "Báo cáo đã được gửi thành công.";
            }
            catch (Exception ex)
            {
                return "Đã xảy ra lỗi khi gửi báo cáo.";
            }
        }

    }
}
