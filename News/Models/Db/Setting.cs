using System;
using System.Collections.Generic;

namespace News.Models.Db;

public partial class Setting
{
    public int Id { get; set; }

    public string? FeaturesNews { get; set; }

    public int? MainNews { get; set; }

    public int? TopStory { get; set; }

    public string? BestNews { get; set; }

    public string? MainPageCategories { get; set; }

    public string? Title { get; set; }

    public string? Logo { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? CopyRight { get; set; }

    public string? Facebook { get; set; }

    public string? Twitter { get; set; }

    public string? Instagram { get; set; }

    public string? Youtube { get; set; }

    public string? Linkedin { get; set; }
}
