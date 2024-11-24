using FinalProject.Data;
using FinalProject.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Services.Admin
{
    public class ResponseListService
    {
        private readonly QlptContext db;

        public ResponseListService(QlptContext db)
        {
            this.db = db;
        }


        public List<ResponseListVM> GetAllResponseList()
        {
            try
            {
                var responseList = (from response in db.Responses
                                    join roomFeedback in db.RoomFeedbacks on response.RoomFeedbackId equals roomFeedback.RoomFeedbackId
                                    join roomPost in db.RoomPosts on roomFeedback.PostId equals roomPost.PostId
                                    join feedback in db.Feedbacks on roomFeedback.FeedbackId equals feedback.FeedbackId
                                    join user in db.Users on roomPost.UserId equals user.UserId
                                    join responseImage in db.ResponseImages on response.ResponseId equals responseImage.ResponseId into responseImages
                                    where roomPost.StatusId == 1
                                    select new ResponseListVM
                                    {
                                        ResponseId = response.ResponseId,
                                        UserName = user.Fullname,
                                        FeedbackName = feedback.FeedbackName,
                                        Address = roomPost.Address,
                                        ResponseText = response.ResponseText,
                                        ResponseDate = response.ResponseDate,
                                        ImageUrls = responseImages.Select(img => img.ImageUrl).ToList()
                                    }).ToList();


                return responseList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public ResponseListVM GetResponseById(int responseId)
        {
            try
            {
                var _response = (from response in db.Responses
                                 join roomFeedback in db.RoomFeedbacks on response.RoomFeedbackId equals roomFeedback.RoomFeedbackId
                                 join roomPost in db.RoomPosts on roomFeedback.PostId equals roomPost.PostId
                                 join feedback in db.Feedbacks on roomFeedback.FeedbackId equals feedback.FeedbackId
                                 join user in db.Users on roomPost.UserId equals user.UserId
                                 where response.ResponseId == responseId
                                 select new ResponseListVM
                                 {
                                     ResponseId = response.ResponseId,
                                     UserName = user.Fullname,
                                     FeedbackName = feedback.FeedbackName,
                                     Address = roomPost.Address,
                                     ResponseText = response.ResponseText,
                                     ResponseDate = response.ResponseDate,
                                     ImageUrls = (from responseImage in db.ResponseImages
                                                  where responseImage.ResponseId == response.ResponseId
                                                  select responseImage.ImageUrl).ToList()
                                 }).FirstOrDefault();


                return _response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool Approve(int responseId)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var response = db.Responses
                        .Where(r => r.ResponseId == responseId)
                        .FirstOrDefault();

                    if (response == null)
                    {
                        return false;
                    }

                    var roomFeedbackId = response.RoomFeedbackId;

                    db.Responses.Remove(response);
                    db.SaveChanges();

                    var roomFeedback = db.RoomFeedbacks
                        .Where(rf => rf.RoomFeedbackId == roomFeedbackId)
                        .FirstOrDefault();

                    if (roomFeedback == null)
                    {
                        return false;
                    }

                    var postId = roomFeedback.PostId;
                    var feedbackId = roomFeedback.FeedbackId;

                    var recordsToDelete = db.RoomFeedbacks
                        .Where(rf => rf.PostId == postId && rf.FeedbackId == feedbackId)
                        .ToList();

                    if (recordsToDelete.Any())
                    {
                        db.RoomFeedbacks.RemoveRange(recordsToDelete);
                        db.SaveChanges();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public bool Reject(int responseId)
        {
            try
            {
                var response = db.Responses
                    .Where(r => r.ResponseId == responseId)
                    .FirstOrDefault();

                if (response == null)
                {
                    return false; 
                }

                db.Responses.Remove(response);
                db.SaveChanges();

                return true; 
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int GetViolatingPostCount()
        {
            return db.RoomPosts.Where(p => p.StatusId == 6).Count();
        }

        public List<RoomPostManagementVM> GetViolatingPost(int pageNumber, int pageSize)
        {
            var query = db.RoomPosts
                .Join(db.Users, rp => rp.UserId, u => u.UserId, (rp, u) => new { rp, u })

                .GroupJoin(db.Bills, combined => combined.rp.PostId, b => b.PostId, (combined, bills) => new { combined.rp, combined.u, bills })
                .SelectMany(combined => combined.bills.DefaultIfEmpty(), (combined, b) => new { combined.rp, combined.u, b })

                .GroupJoin(db.Services, combined => combined.b == null ? (int?)null : combined.b.ServiceId, s => s.ServiceId, (combined, services) => new { combined.rp, combined.u, combined.b, services })
                .SelectMany(combined => combined.services.DefaultIfEmpty(), (combined, s) => new { combined.rp, combined.u, combined.b, s })

                .Select(combined => new
                {
                    combined.rp,
                    combined.u,
                    combined.b,
                    combined.s,
                    RoomImage = db.RoomImages
                                    .Where(ri => ri.PostId == combined.rp.PostId)
                                    .Select(ri => ri.ImageUrl)
                                    .FirstOrDefault()
                })

                .Join(db.RoomStatuses, combined => combined.rp.StatusId, rs => rs.RoomStatusId, (combined, rs) => new { combined.rp, combined.u, combined.b, combined.s, combined.RoomImage, rs })
                .Where(p => p.b == null || p.b.ExpirationDate.HasValue)
                .Where(p => p.rp.StatusId == 6)
                .AsEnumerable()
                .Where(p => p.b == null || p.b.ExpirationDate.Value.ToDateTime(new TimeOnly()) >= DateTime.Now)
                .OrderBy(p => p.rp.PostId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new RoomPostManagementVM
                {
                    PostId = (int)p.rp.PostId,
                    RoomImage = p.RoomImage,
                    Address = p.rp.Address,
                    UserId = p.u.UserId,
                    FullName = p.u.Fullname,
                    ServiceId = p.s != null ? p.s.ServiceId : 0,
                    ServiceName = p.s != null ? p.s.ServiceName : "Không có dịch vụ",
                    StatusId = (int)p.rp.StatusId,
                    StatusName = p.rs.StatusName
                })
                .ToList();

            return query;
        }

        public List<RoomPostManagementVM> SearchViolatingPost(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return new List<RoomPostManagementVM>();
            }

            var query = db.RoomPosts
                .Join(db.Users, rp => rp.UserId, u => u.UserId, (rp, u) => new { rp, u })
                .Join(db.Bills, combined => combined.rp.PostId, b => b.PostId, (combined, b) => new { combined.rp, combined.u, b })

                .GroupJoin(db.Services, combined => combined.b.ServiceId, s => s.ServiceId, (combined, services) => new { combined.rp, combined.u, combined.b, services })
                .SelectMany(
                    combined => combined.services.DefaultIfEmpty(),
                    (combined, s) => new { combined.rp, combined.u, combined.b, s })

                .Select(combined => new
                {
                    combined.rp,
                    combined.u,
                    combined.b,
                    combined.s,
                    RoomImage = db.RoomImages
                                    .Where(ri => ri.PostId == combined.rp.PostId)
                                    .Select(ri => ri.ImageUrl)
                                    .FirstOrDefault()
                })
                .Join(db.RoomStatuses, combined => combined.rp.StatusId, rs => rs.RoomStatusId, (combined, rs) => new { combined.rp, combined.u, combined.b, combined.s, combined.RoomImage, rs })
                .Where(p => p.b.ExpirationDate.HasValue)
                .Where(p => p.rp.StatusId == 6)
                .AsEnumerable()
                .Where(p => p.b.ExpirationDate.Value.ToDateTime(new TimeOnly()) >= DateTime.Now)
                .Where(p => p.rp.RoomName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            p.rp.Address.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            p.u.Fullname.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            p.s.ServiceName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            p.rs.StatusName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.rp.PostId)
                .Select(p => new RoomPostManagementVM
                {
                    PostId = (int)p.rp.PostId,
                    RoomImage = p.RoomImage,
                    Address = p.rp.Address,
                    UserId = p.u.UserId,
                    FullName = p.u.Fullname,
                    ServiceId = p.s?.ServiceId ?? 0,
                    ServiceName = p.s?.ServiceName ?? "Không có dịch vụ",
                    StatusId = (int)p.rp.StatusId,
                    StatusName = p.rs.StatusName
                })
                .ToList();

            return query;
        }
    }
}
