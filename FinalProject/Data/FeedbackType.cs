using System;
using System.Collections.Generic;

namespace FinalProject.Data;

public partial class FeedbackType
{
    public int FeedbackTypeId { get; set; }

    public string? TypeName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
