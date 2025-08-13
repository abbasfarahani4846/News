using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;

using static System.Net.Mime.MediaTypeNames;

namespace News.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsController : Controller
    {
        private readonly NewsContext _context;

        public NewsController(NewsContext context)
        {
            _context = context;
        }

        // GET: Admin/News
        public async Task<IActionResult> Index()
        {
            return View(await _context.News.ToListAsync());
        }

        // GET: Admin/News/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
            ViewBag.Tags = new SelectList(await _context.Tags.ToListAsync(), "Title", "Title");

            return View();
        }

        // POST: Admin/News/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ShortDescription,LongDescription,CreatedAt,ViewCount,Status,IsTrend,ImageName,CategoryId")] News.Models.Db.News news, IFormFile image, string[] tags)
        {
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    news.ImageName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    //------------------------------------------------
                    string d = Directory.GetCurrentDirectory();
                    string fn = d + "\\wwwroot\\images\\news\\" + news.ImageName;

                    //------------------------------------------------
                    using (var stream = new FileStream(fn, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }
                }
                else
                {
                    ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
                    ViewBag.Tags = new SelectList(await _context.Tags.ToListAsync(), "Title", "Title");

                    ModelState.AddModelError("Title", "Upload a image");
                    return View(news);
                }

                news.Tags = tags != null ? string.Join(",", tags) : "";
                news.CreatedAt = DateTime.Now;
                news.ViewCount = 0;

                _context.Add(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
            ViewBag.Tags = new SelectList(await _context.Tags.ToListAsync(), "Title", "Title");
            return View(news);
        }

        // GET: Admin/News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
            ViewBag.Tags = new SelectList(await _context.Tags.ToListAsync(), "Title", "Title",news.Tags?.Split(",").ToList());

            return View(news);
        }

        // POST: Admin/News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ShortDescription,LongDescription,CreatedAt,ViewCount,Status,IsTrend,ImageName")] News.Models.Db.News news, IFormFile? image)
        {
            if (id != news.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null)
                    {
                        //------------------------------------------------
                        string d = Directory.GetCurrentDirectory();
                        string fn = d + "\\wwwroot\\images\\news\\";

                        //------------------------------------------------
                        if (System.IO.File.Exists(fn + news.ImageName))
                        {
                            System.IO.File.Delete(fn + news.ImageName);
                        }
                        //------------------------------------------------
                        news.ImageName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                        fn += news.ImageName;

                        using (var stream = new FileStream(fn, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }
                    }

                    _context.Update(news);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
            ViewBag.Tags = new SelectList(await _context.Tags.ToListAsync(), "Title", "Title");
            return View(news);
        }

        // GET: Admin/News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: Admin/News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                string d = Directory.GetCurrentDirectory();
                string fn = d + "\\wwwroot\\images\\news\\" + news.ImageName;

                //------------------------------------------------
                if (System.IO.File.Exists(fn))
                {
                    System.IO.File.Delete(fn);
                }

                _context.News.Remove(news);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload?.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(upload.FileName)}";
                var folderPath = Path.Combine("images", "news"); // مسیر نسبی داخل wwwroot
                var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderPath);

                Directory.CreateDirectory(saveDir); // ساخت پوشه در صورت نبود

                var filePath = Path.Combine(saveDir, fileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await upload.CopyToAsync(stream);

                // آدرس برای مرورگر
                var url = $"/{folderPath.Replace("\\", "/")}/{fileName}";
                return Json(new { url });
            }

            return BadRequest(new { message = "No file uploaded." });
        }


    }
}
