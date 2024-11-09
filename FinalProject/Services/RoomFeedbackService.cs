﻿using FinalProject.Data;
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

            var posts = db.RoomPosts
                          .Where(t => t.UserId == userId && t.StatusId != 4)
                          .ToList();

            var postIds = posts.Select(p => (int?)p.PostId).ToList();

            var roomFeedbacks = db.RoomFeedbacks
                                  .Where(t => postIds.Contains(t.PostId))  
                                  .GroupBy(t => new { t.PostId, t.FeedbackId })
                                  .Select(g => new
                                  {
                                      PostId = g.Key.PostId,
                                      FeedbackId = g.Key.FeedbackId,
                                      ViolationCount = g.Count()
                                  })
                                  .ToList();

            var feedbackIds = roomFeedbacks.Select(rf => rf.FeedbackId).Distinct().ToList();
            var feedbacks = db.Feedbacks
                              .Where(f => feedbackIds.Contains(f.FeedbackId))
                              .ToDictionary(f => f.FeedbackId);

            foreach (var item in roomFeedbacks)
            {
                if (item.ViolationCount >= 5 && item.ViolationCount <= 10)
                {
                    var post = posts.FirstOrDefault(p => p.PostId == item.PostId);
                    if (post != null && feedbacks.TryGetValue((int)item.FeedbackId, out var feedback))
                    {
                        FeedbackResultVM feedbackResultVM = new FeedbackResultVM
                        {
                            ViolationCount = item.ViolationCount,
                            Address = post.Address,
                            FeedbackName = feedback.FeedbackName
                        };
                        ds.Add(feedbackResultVM);
                    }
                }
            }
            return ds;
        }

        public void HidePostsForAllViolationsAsync()
        {
            var postsToHide = db.RoomPosts
                                .Where(p => p.StatusId != 4)  
                                .ToList();

            var postsWithViolations7 = db.RoomFeedbacks
                                          .GroupBy(rf => rf.PostId)
                                          .Select(g => new
                                          {
                                              PostId = g.Key,
                                              ViolationCount = g.Count()
                                          })
                                          .Where(g => g.ViolationCount == 7) 
                                          .ToList();

            var postsToHide7 = postsWithViolations7.Select(p => p.PostId).ToList();


            foreach (var postId in postsToHide7)
            {
                var post = db.RoomPosts.FirstOrDefault(p => p.PostId == postId);
                if (post != null && post.StatusId != 4) 
                {
                    HidePost((int)postId);

                    LockAccount(post.UserId);
                }
            }
        }


        private void HidePost(int postId)
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
            try
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.IsValid = false;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

                return "Báo cáo đã được gửi thành công.";
            }
            catch (Exception ex)
            {
                return "Đã xảy ra lỗi khi gửi báo cáo.";
            }
        }

    }
}
