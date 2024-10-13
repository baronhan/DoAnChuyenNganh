using System;
using System.Collections.Generic;

namespace FinalProject.Data;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? Fullname { get; set; }

    public string Password { get; set; } = null!;

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public string? UserImage { get; set; }

    public string? RandomKey { get; set; }
    public bool IsValid { get; set; }

    public int? UserTypeId { get; set; }

    public virtual UserType? UserType { get; set; }
}
