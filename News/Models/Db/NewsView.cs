using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class NewsView
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string ShortDescription { get; set; } = null!;

    public string LongDescription { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? ViewCount { get; set; }

    public string Status { get; set; } = null!;

    public string? ImageName { get; set; }

    public int? CategoryId { get; set; }

    public string? Tags { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;
}
