namespace News.Models.ViewModels
{
    public class FooterViewModel
    {
        public News.Models.Db.Setting Settings { get; set; } = new News.Models.Db.Setting();
        public List<News.Models.Db.News> RecentPosts { get; set; } = new List<Db.News>();
        public List<News.Models.Db.News> Gallary { get; set; } = new List<Db.News>();
        public List<News.Models.Db.Category> Categories { get; set; } = new List<Db.Category>();
    }
}
