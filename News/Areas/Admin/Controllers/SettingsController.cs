using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;

namespace News.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly NewsContext _context;

        public SettingsController(NewsContext context)
        {
            _context = context;
        }

        // This is the GET action that displays the edit form.
        public async Task<IActionResult> Edit()
        {
            // Since there's likely only one row in the settings table, we get the first one.
            var setting = await _context.Settings.FirstOrDefaultAsync();
            if (setting == null)
            {
                // If no settings row exists yet, you might want to create one or show an error.
                // For now, returning NotFound is acceptable.
                return NotFound();
            }

            // --- Load details for single-select fields ---

            // Load details for MainNews
            if (setting.MainNews.HasValue && setting.MainNews.Value > 0)
            {
                ViewBag.MainNewsDetails = await _context.News.FindAsync(setting.MainNews.Value);
            }

            // Load details for TopStory
            if (setting.TopStory.HasValue && setting.TopStory.Value > 0)
            {
                ViewBag.TopStoryDetails = await _context.News.FindAsync(setting.TopStory.Value);
            }

            // --- Load details for multi-select fields ---

            // Load details for FeaturesNews
            if (!string.IsNullOrEmpty(setting.FeaturesNews))
            {
                var featuresNewsIds = setting.FeaturesNews.Split(',').Select(int.Parse).ToList();
                ViewBag.FeaturesNewsDetails = await _context.News
                                                      .Where(n => featuresNewsIds.Contains(n.Id))
                                                      .ToListAsync();
            }

            // Load details for BestNews
            if (!string.IsNullOrEmpty(setting.BestNews))
            {
                var bestNewsIds = setting.BestNews.Split(',').Select(int.Parse).ToList();
                ViewBag.BestNewsDetails = await _context.News
                                                  .Where(n => bestNewsIds.Contains(n.Id))
                                                  .ToListAsync();
            }

            if (!string.IsNullOrEmpty(setting.MainPageCategories))
            {
                var categoryIds = setting.MainPageCategories.Split(',').Select(int.Parse).ToList();
                ViewBag.MainPageCategoriesDetails = await _context.Categories
                                                          .Where(c => categoryIds.Contains(c.Id))
                                                          .ToListAsync();
            }

            // Pass the setting object to the view to bind the form fields.
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Setting setting, List<string> FeaturesNews, List<string> BestNews, List<string> MainPageCategories)
        {
            // The model state will be validated against the 'setting' object's simple properties.
            if (ModelState.IsValid)
            {
                try
                {
                    // 3. Fetch the original entity from the database to update it.
                    var settingToUpdate = await _context.Settings.FirstOrDefaultAsync();
                    if (settingToUpdate == null)
                    {
                        return NotFound();
                    }

                    // 4. Update the simple properties from the bound 'setting' object.
                    settingToUpdate.MainNews = setting.MainNews;
                    settingToUpdate.TopStory = settingToUpdate.TopStory;

                    // 5. Manually join the lists and update the multi-select properties.
                    //    The model binder has already correctly populated 'FeaturesNews' and 'BestNews' lists.
                    settingToUpdate.FeaturesNews = (FeaturesNews != null) ? string.Join(",", FeaturesNews) : "";
                    settingToUpdate.BestNews = (BestNews != null) ? string.Join(",", BestNews) : "";

                    settingToUpdate.MainPageCategories = (MainPageCategories != null) ? string.Join(",", MainPageCategories) : "";


                    // 6. Save the changes.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SettingExists(setting.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Edit");
            }

            // If validation fails, repopulate ViewBag and return the view.
            // ... your code to reload dropdown data ...
            return RedirectToAction("Edit");
        }


        // In your NewsController.cs or another appropriate controller
        [HttpGet]
        public async Task<IActionResult> SearchNews(string q) // 'q' is the default parameter name for the search term
        {
            // If the search term is empty, return no results.
            if (string.IsNullOrEmpty(q))
            {
                return Json(new { results = new List<object>() });
            }

            // Search the database for news titles that contain the search term.
            // The query is case-insensitive.
            var newsQuery = _context.News
                                  .Where(n => n.Title.ToLower().Contains(q.ToLower()));

            // Project the results into the format required by Select2.
            // Limit the results to the top 10 for performance.
            var results = await newsQuery
                                .Select(n => new { id = n.Id, text = n.Title })
                                .Take(10)
                                .ToListAsync();

            // Return the results in the { results: [...] } structure that Select2 expects.
            return Json(new { results });
        }


        [HttpGet]
        public async Task<IActionResult> SearchCategories(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return Json(new { results = new List<object>() });
            }

            // Assuming your Category entity has 'Id' and 'Title' properties
            var categories = await _context.Categories
                                           .Where(c => c.Title.ToLower().Contains(q.ToLower()))
                                           .Select(c => new { id = c.Id, text = c.Title })
                                           .Take(10)
                                           .ToListAsync();

            return Json(new { results = categories });
        }

        private bool SettingExists(int id)
        {
            return _context.Settings.Any(e => e.Id == id);
        }
    }
}
