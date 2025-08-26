namespace News.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int NewsCount { get; set; }
        public int CommentsCount { get; set; }
        public int CategoriesCount { get; set; }
        public int TagsCount { get; set; }

        // For X-axis labels (e.g., "Jan", "Feb", ...)
        public List<string> ChartLabels { get; set; } = new List<string>();

        // For the chart's data points (count of news per month)
        public List<int> ChartData { get; set; } = new List<int>();
    }
}
