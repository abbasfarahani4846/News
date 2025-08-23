using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class Subscriber
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public DateTime SubscribedAt { get; set; }

    public bool IsActive { get; set; }
}
