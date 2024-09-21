using System;
using System.Collections.Generic;

namespace FinalProject.Data;

public partial class RoomImage
{
    public int ImageId { get; set; }

    public int? PostId { get; set; }

    public int? ImageTypeId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ImageType? ImageType { get; set; }

    public virtual RoomPost? Post { get; set; }
}
