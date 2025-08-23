using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;
using News.Models.ViewModels;

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

            result.Categories = await _context.Categories.ToListAsync();

            result.Settings = await _context.Settings.FirstOrDefaultAsync();

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
