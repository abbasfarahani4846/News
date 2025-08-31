namespace News.Models.ViewModels
{
    public class ChartViewModel
    {
        // Holds the labels for the X-axis (e.g., month names).
        public List<string> Labels { get; set; } = new List<string>();

        // Holds the data points for the Y-axis (e.g., count of news).
        public List<int> Data { get; set; } = new List<int>();
    }
}
