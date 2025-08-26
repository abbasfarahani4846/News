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
            AdminDashboardViewModel result = new AdminDashboardViewModel
            {
                NewsCount = await _context.News.CountAsync(),
                CommentsCount = await _context.Comments.CountAsync(),
                CategoriesCount = await _context.Categories.CountAsync(),
                TagsCount = await _context.Tags.CountAsync()
            };

            // --- Part 2: Calculate data for the chart ---

            // Get all news published in the current year.
            var newsThisYear = await _context.News
                                       .Where(n => n.CreatedAt.Year == DateTime.Now.Year)
                                       .ToListAsync();

            // Group the news by month and count them.
            var monthlyCounts = newsThisYear
                                .GroupBy(n => n.CreatedAt.Month)
                                .ToDictionary(g => g.Key, g => g.Count());

            // Loop through all 12 months to populate chart data.
            for (int i = 1; i <= 12; i++)
            {
                // Add the month's abbreviated name to the labels (e.g., "Jan").
                result.ChartLabels.Add(CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(i));

                // If there's data for this month, add the count; otherwise, add 0.
                if (monthlyCounts.ContainsKey(i))
                {
                    result.ChartData.Add(monthlyCounts[i]);
                }
                else
                {
                    result.ChartData.Add(0);
                }
            }

            // Return the view with the complete ViewModel (counts + chart data).

            return View(result);
        }
    }
}
