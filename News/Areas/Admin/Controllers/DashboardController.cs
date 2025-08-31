using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;
using News.Models.ViewModels;

using System.Globalization;

namespace News.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly NewsContext _context;

        public DashboardController(NewsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // --- Part 1: Calculate the total counts for the stat cards ---
            // This part is efficient and remains unchanged.
            AdminDashboardViewModel result = new AdminDashboardViewModel
            {
                NewsCount = await _context.News.CountAsync(),
                CommentsCount = await _context.Comments.CountAsync(),
                CategoriesCount = await _context.Categories.CountAsync(),
                TagsCount = await _context.Tags.CountAsync()
            };

            // --- Part 2: OPTIMIZED - Calculate monthly data for the chart ---
            int currentYear = DateTime.Now.Year;

            // Query 1: Get the count of news published per month for the current year.
            // This entire query is executed on the database server for maximum efficiency.
            var monthlyNewsCounts = await _context.News
                .Where(n => n.CreatedAt.Year == currentYear) // Filter by the current year on the DB.
                .GroupBy(n => n.CreatedAt.Month)             // Group records by month number on the DB.
                .Select(g => new { Month = g.Key, Count = g.Count() }) // Select the month and its count.
                .ToDictionaryAsync(item => item.Month, item => item.Count); // Fetch data and convert to a dictionary for fast lookups.

            // --- NEW ---
            // Query 2: Get the count of comments created per month for the current year.
            // This follows the same efficient pattern as the news query.
            var monthlyCommentCounts = await _context.Comments
                .Where(c => c.CreatedAt.Year == currentYear) // Assuming the Comment model also has a CreatedAt property.
                .GroupBy(c => c.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(item => item.Month, item => item.Count);


            // --- Part 3: Populate the ViewModel with the chart results ---
            // We loop through all 12 months to ensure our chart has a complete X-axis.
            for (int month = 1; month <= 12; month++)
            {
                // Add the month's abbreviated name (e.g., "Jan") to the chart labels.
                result.ChartLabels.Add(CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month));

                // Populate the news data for the current month.
                // We use TryGetValue for safety, which returns the count if the key exists, or 0 otherwise.
                monthlyNewsCounts.TryGetValue(month, out int newsCount);
                result.ChartData.Add(newsCount);

                // --- NEW ---
                // Populate the comment data for the current month.
                // We do the same for comments. If no comments were made in a month, we add 0.
                monthlyCommentCounts.TryGetValue(month, out int commentCount);
                result.ChartDataComments.Add(commentCount);
            }

            // Return the view with the fully populated ViewModel.
            return View(result);
        }

    }
}
