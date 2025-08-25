using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;

namespace News.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly NewsContext _context;

        public NavbarViewComponent(NewsContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = await _context.Menus.Where(x => x.Position == "top").ToListAsync();
            var trends = await _context.News.Where(x => x.IsTrend && x.Status == "PUBLISH").OrderBy(x => x.CreatedAt).Take(5).ToListAsync();
            var setting = await _context.Settings.FirstOrDefaultAsync();

            var result = Tuple.Create(menuItems, trends,setting);

            return View(result);
        }
    }
}
