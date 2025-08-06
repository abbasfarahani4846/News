using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class Tag
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;
}
