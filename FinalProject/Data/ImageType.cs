using System;
using System.Collections.Generic;

namespace FinalProject.Data;

public partial class ImageType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
}
