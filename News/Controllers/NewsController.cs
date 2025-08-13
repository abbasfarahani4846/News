using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using News.Models.Db;
using News.Models.Helpers;
using News.Models.ViewModels;

namespace News.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsContext _context;

        public NewsController(NewsContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> NewsDetails(int newsId)
        {
            var news = await _context.News.FirstOrDefaultAsync(x => x.Id == newsId);
            if (news == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments.Where(x => x.NewsId == newsId).ToListAsync();

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == news.CategoryId);

            var relatedNews = await _context.News.Where(x => x.CategoryId == category.Id && x.Id != news.Id).Take(2).ToListAsync();

            var result = new NewsDetailsViewModel()
            {
                News = news,
                Comments = comments,
                Category = category,
                ReadingTimeInMinutes = TextHelpers.CalculateReadingTime(news.LongDescription),
                RelatedNews = relatedNews
            };

            return View(result);
        }
    }
}
