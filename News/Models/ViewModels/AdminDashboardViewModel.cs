using News.Models.Db;

namespace News.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // --- Aggregate Counts for Stat Cards ---
        public int NewsCount { get; set; }
        public int CommentsCount { get; set; }
        public int CategoriesCount { get; set; }
        public int TagsCount { get; set; }

        // --- Chart Data ---

        /// <summary>
        /// Holds the labels for the chart's X-axis (e.g., "Jan", "Feb", "Mar").
        /// This list is shared by all datasets in the chart.
        /// </summary>
        public List<string> ChartLabels { get; set; } = new List<string>();

        /// <summary>
        /// Holds the data points for the "Published News" line on the chart.
        /// Each integer corresponds to a label in ChartLabels.
        /// </summary>
        public List<int> ChartData { get; set; } = new List<int>();

        /// <summary>
        /// --- NEW ---
        /// Holds the data points for the "Published Comments" line on the chart.
        /// Each integer corresponds to a label in ChartLabels.
        /// </summary>
        public List<int> ChartDataComments { get; set; } = new List<int>();
    }
}
