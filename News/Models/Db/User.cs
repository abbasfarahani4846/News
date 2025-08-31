using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsAdmin { get; set; }
}
