using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;
using News.Models.ViewModels;

using NuGet.Configuration;

namespace News.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly NewsContext _context;

        public FooterViewComponent(NewsContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = new FooterViewModel();

            result.Settings = await _context.Settings.FirstAsync();

            // Step 1: Get the list of selected category IDs from the settings string.
            var mainPageCategoriesIds = result.Settings.MainPageCategories.Split(',').Select(int.Parse).ToList();

            // Step 2: Fetch all the required Category objects in a single query.
            result.Categories = await _context.Categories
                                             .Where(c => mainPageCategoriesIds.Contains(c.Id))
                                             .ToListAsync();

            result.RecentPosts = await _context.News
                .Where(x => x.Status == "PUBLISH")
                .OrderByDescending(x => x.CreatedAt)
                .Take(2)
                .ToListAsync();

            result.Gallary = await _context.News
                               .Where(x => x.Status == "PUBLISH")
                               .OrderBy(n => Guid.NewGuid())
                               .Take(6)
                               .ToListAsync();

            return View(result);
        }
    }
}
