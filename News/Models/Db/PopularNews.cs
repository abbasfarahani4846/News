using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class PopularNews
{
    public int? CommentCount { get; set; }

    public int? Id { get; set; }

    public string? Title { get; set; }

    public string? LongDescription { get; set; }

    public string? ShortDescription { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ViewCount { get; set; }

    public string? Status { get; set; }

    public string? ImageName { get; set; }

    public int? CategoryId { get; set; }

    public string? Tags { get; set; }

    public int? UserId { get; set; }
}
