using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class PopularCategory
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int? NewsCount { get; set; }
}
