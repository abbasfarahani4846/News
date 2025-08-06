using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class News
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string ShortDescription { get; set; } = null!;

    public string LongDescription { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? ViewCount { get; set; }

    public int Status { get; set; }

    public bool IsTrend { get; set; }

    public string? ImageName { get; set; }

    public int CategoryId { get; set; }
}
