using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Index(string searchTerm, int? categoryId, string tag)
        {
            // --- Start with a base IQueryable ---
            // An IQueryable doesn't execute the query immediately.
            // It allows us to build up the query step-by-step.
            IQueryable<News.Models.Db.News> query = _context.News.AsQueryable();

            // --- Apply filters conditionally ---

            // 1. Filter by Search Term (Title)
            // If a search term is provided, add a Where clause to filter by title.
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(n => n.Title.ToLower().Contains(searchTerm.ToLower()));
            }

            // 2. Filter by Category
            // If a categoryId is provided, add a Where clause to filter by that category.
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(n => n.CategoryId == categoryId.Value);
            }

            // 3. Filter by Tag
            // If a tag is provided, add a Where clause to filter by tags.
            // This checks if the comma-separated 'Tags' string contains the given tag.
            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(n => n.Tags.ToLower().Contains(tag.ToLower()));
            }

            // --- Prepare data for the view ---

            // Load the list of categories to populate the filter dropdown in the view.
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title", categoryId);

            // Pass the current filter values back to the view, so the search boxes don't clear after searching.
            ViewBag.CurrentSearchTerm = searchTerm;
            ViewBag.CurrentTag = tag;

            // --- Execute the query and return the result ---
            // Order the final results by creation date (newest first).
            // The query is only executed here, when ToListAsync() is called.
            var filteredNews = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();

            return View(filteredNews);
        }

        [HttpGet("news/{id}")]
        public async Task<IActionResult> NewsDetails(int id)
        {
            var news = await _context.News.FirstOrDefaultAsync(x => x.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            news.ViewCount++;
            _context.News.Update(news);
            await _context.SaveChangesAsync();

            var comments = await _context.Comments.Where(x => x.NewsId == id).OrderByDescending(x => x.Id).ToListAsync();

            var categories = await _context.Categories.ToListAsync();

            var category = categories.FirstOrDefault(x => x.Id == news.CategoryId);

            var relatedNews = await _context.News.Where(x => x.CategoryId == category.Id && x.Id != news.Id).Take(2).ToListAsync();

            var currentNewsId = news.Id;

            var popularNews = await _context.PopularNews.OrderByDescending(x => x.CommentCount).Take(5).ToListAsync();


            var result = new NewsDetailsViewModel()
            {
                News = news,
                Comments = comments,
                Category = category,
                ReadingTimeInMinutes = TextHelpers.CalculateReadingTime(news.LongDescription),
                RelatedNews = relatedNews,
                Categories = categories,
                Popular = popularNews
            };


            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComment(Comment commentModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set server-side properties securely.
                    commentModel.CreatedAt = DateTime.Now;
                    commentModel.IsApproved = true;

                    // Add the new comment to the context and save to the database.
                    _context.Comments.Add(commentModel);
                    await _context.SaveChangesAsync();

                    // Redirect the user back to the news article they were on.
                    // You can add a success message using TempData here.
                    TempData["SuccessMessage"] = "Your comment has been submitted and is awaiting approval.";
                    return Redirect("/news/" + commentModel.NewsId + "#comment-form");
                }
                catch
                {
                    // In case of a database error, add an error message and redirect.
                    TempData["ErrorMessage"] = "There was an error submitting your comment. Please try again.";
                    return Redirect("/news/" + commentModel.NewsId + "#comment-form");
                }
            }

            // If ModelState is not valid, it means some required fields were empty.
            // Redirect back to the page to show the validation errors.
            TempData["ErrorMessage"] = "Please fill in all required fields.";
            return Redirect("/news/" + commentModel.NewsId + "#comment-form");
        }
    }
}
