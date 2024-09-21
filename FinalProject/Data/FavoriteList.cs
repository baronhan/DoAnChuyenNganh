using System;
using System.Collections.Generic;

namespace FinalProject.Data;

public partial class FavoriteList
{
    public int FavoriteListId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<FavoriteListPost> FavoriteListPosts { get; set; } = new List<FavoriteListPost>();
}
