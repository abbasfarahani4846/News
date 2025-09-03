using News.Models.Db;

using System.Collections.Generic;

namespace News.Models.ViewModels
{
    public class HomeViewModel
    {
        // For single news items like MainNews and TopStory
        public News.Models.Db.News? MainNews { get; set; }
        public News.Models.Db.News? TopStory { get; set; }

        // For lists of news items like FeaturesNews and BestNews
        public List<News.Models.Db.News> FeaturesNews { get; set; } = new List<News.Models.Db.News>();
        public List<News.Models.Db.News> BestNews { get; set; } = new List<News.Models.Db.News>();
        public List<News.Models.Db.NewsView> LastNews { get; set; } = new List<News.Models.Db.NewsView>();
        public List<News.Models.Db.NewsView> MostViewsNews { get; set; } = new List<News.Models.Db.NewsView>();

        public List<MainPageCategoryViewModel> MainPageCategories { get; set; } = new List<MainPageCategoryViewModel>();


    }

    public class MainPageCategoryViewModel
    {
        public News.Models.Db.Category? Category { get; set; }
        public List<News.Models.Db.News> News { get; set; } = new List<News.Models.Db.News>();
    }
}
