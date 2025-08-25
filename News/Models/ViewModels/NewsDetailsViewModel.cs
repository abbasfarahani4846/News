using News.Models.Db;

namespace News.Models.ViewModels
{
    public class NewsDetailsViewModel
    {
        public News.Models.Db.News News { get; set; }
        public News.Models.Db.Category Category { get; set; } = new News.Models.Db.Category();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int ReadingTimeInMinutes { get; set; }
        public List<News.Models.Db.News> RelatedNews { get; set; } = new List<News.Models.Db.News>();
        public List<News.Models.Db.News> Popular { get; set; } = new List<News.Models.Db.News>();
        public List<News.Models.Db.Category> Categories { get; set; } = new List<News.Models.Db.Category>();
    }
}
