using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class NewsTag
{
    public int Id { get; set; }

    public int NewsId { get; set; }

    public int TagId { get; set; }
}
