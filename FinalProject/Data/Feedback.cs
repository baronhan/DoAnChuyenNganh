using System;
using System.Collections.Generic;

namespace FinalProject.Data;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? UserId { get; set; }

    public int? PostId { get; set; }

    public string? Status { get; set; }

    public int? FeedbackTypeId { get; set; }

    public virtual FeedbackType? FeedbackType { get; set; }

    public virtual RoomPost? Post { get; set; }
}
