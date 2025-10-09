using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;
using News.Models.Helpers;

using static System.Net.Mime.MediaTypeNames;

namespace News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class NewsController : Controller
    {
        private readonly NewsContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public NewsController(NewsContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/News
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadNewsData()
        {
            try
            {
                // --- 1. Read parameters sent from DataTables ---
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                // --- 2. Convert parameters to correct types ---
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // --- 3. Start the main query ---
                IQueryable<News.Models.Db.News> query = _context.News.AsQueryable();

                // if is  not admin, show only their news
                if (!User.IsInRole("Admin"))
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString());
                    query = query.Where(news => news.UserId == userId);
                }

                // --- 4. Get total records count (before filtering) ---
                var recordsTotal = await query.CountAsync();

                // --- 5. Apply search filter ---
                if (!string.IsNullOrEmpty(searchValue))
                {
                    var searchValueLower = searchValue.ToLower();
                    query = query.Where(n =>
                        n.Title.ToLower().Contains(searchValueLower) ||
                        n.Status.ToLower().Contains(searchValueLower));
                }

                // --- 6. Get filtered records count ---
                var recordsFiltered = await query.CountAsync();

                // --- 7. Apply sorting ---
                // Note: A simple implementation. A more robust solution would be more dynamic.
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    if (sortColumnDirection == "asc")
                    {
                        switch (sortColumn.ToLower())
                        {
                            case "title":
                                query = query.OrderBy(n => n.Title);
                                break;
                            case "viewcount":
                                query = query.OrderBy(n => n.ViewCount);
                                break;
                                // Add other columns here
                        }
                    }
                    else
                    {
                        switch (sortColumn.ToLower())
                        {
                            case "title":
                                query = query.OrderByDescending(n => n.Title);
                                break;
                            case "viewcount":
                                query = query.OrderByDescending(n => n.ViewCount);
                                break;
                                // Add other columns here
                        }
                    }
                }

                // --- 8. Apply pagination ---
                var pagedData = await query.Skip(skip).Take(pageSize).ToListAsync();

                // --- 9. Build and return the response in the standard DataTables format ---
                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsFiltered,
                    recordsTotal = recordsTotal,
                    data = pagedData
                };

                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                // Log the exception
                return BadRequest();
            }
        }


        // GET: Admin/News/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
            ViewBag.Tags = new SelectList(await _context.Tags.ToListAsync(), "Title", "Title");
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "FullName");

            return View();
        }

        // POST: Admin/News/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ShortDescription,LongDescription,CreatedAt,ViewCount,Status,ImageName,CategoryId,UserId")] News.Models.Db.News news, IFormFile ImageName, string[] tags)
        {
            if (ModelState.IsValid)
            {
                // The code inside your [HttpPost] Create action method
                if (ImageName != null)
                {
                    // --- 1. Generate a new unique file name ---
                    // This prevents file name conflicts.
                    var newImageName = Guid.NewGuid().ToString() + Path.GetExtension(ImageName.FileName);

                    // --- 2. Define the paths for the original image and its thumbnail ---
                    // Use Path.Combine for cross-platform safety and correctness.
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "news", newImageName);
                    var thumbnailPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "news", "thumb_" + newImageName);

                    // --- 3. Save original and create thumbnail efficiently from one stream ---
                    // Open the uploaded file's stream just once.
                    await using (var stream = ImageName.OpenReadStream())
                    {
                        // a) Save the original image by copying the stream to a file.
                        // Use the async version of CopyTo.
                        await using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }

                        // b) Reset the stream's position to the beginning to read it again.
                        stream.Position = 0;

                        // c) Create the thumbnail from the same stream without extra disk I/O.
                        ImageHelper.CreateThumbnail(stream, thumbnailPath, 400);
                    }

                    // 4. Update the entity with the new image name before saving.
                    news.ImageName = newImageName;
                }
                else
                {
                    // If no image was uploaded, add a model error and return to the view.
                    // The error is now correctly associated with the 'MainImage' property.
                    ModelState.AddModelError("ImageName", "Please upload an image for the news.");

                    // Repopulate dropdowns before returning the view.
                    ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
                    ViewBag.Tags = new SelectList(await _context.Tags.ToListAsync(), "Id", "Title");

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
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "FullName");
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

            var selectedTagIds = news.Tags?.Split(',').ToList() ?? new List<string>();
            ViewBag.Tags = new MultiSelectList(await _context.Tags.ToListAsync(), "Title", "Title", selectedTagIds);
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "FullName", news.UserId);

            return View(news);
        }

        // POST: Admin/News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ShortDescription,LongDescription,CreatedAt,ViewCount,Status,ImageName,CategoryId,UserId")] News.Models.Db.News news, IFormFile? image, string[] tags)
        {
            if (id != news.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ... inside your action method ...
                    if (image != null) // Assuming 'image' is your IFormFile
                    {
                        // --- 1. Delete Old Files (if they exist) ---
                        // First, check if there's an old image name to delete.
                        if (!string.IsNullOrEmpty(news.ImageName))
                        {
                            // Construct the full path for the old image and thumbnail.
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "news", news.ImageName);
                            var oldThumbnailPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "news", "thumb_" + news.ImageName);

                            // Delete the files if they exist.
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            if (System.IO.File.Exists(oldThumbnailPath))
                            {
                                System.IO.File.Delete(oldThumbnailPath);
                            }
                        }

                        // --- 2. Generate New Unique File Name ---
                        // Create a new unique name for the image to avoid conflicts.
                        var newImageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        news.ImageName = newImageName; // Update the entity with the new name.

                        // --- 3. Define Paths for New Files ---
                        // Construct the full paths for the new image and its thumbnail.
                        var newImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "news", newImageName);
                        var newThumbnailPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "news", "thumb_" + newImageName);

                        // --- 4. Save Original and Create Thumbnail from a single stream ---
                        // Open the uploaded file's stream once.
                        await using (var stream = image.OpenReadStream())
                        {
                            // a) Save the original image by copying the stream to a new file.
                            await using (var fileStream = new FileStream(newImagePath, FileMode.Create))
                            {
                                await stream.CopyToAsync(fileStream);
                            }

                            // b) Reset the stream's position to the beginning.
                            stream.Position = 0;

                            // c) Create the thumbnail from the same stream without re-reading from disk.
                            ImageHelper.CreateThumbnail(stream, newThumbnailPath, 400);
                        }
                    }

                    news.Tags = tags != null ? string.Join(",", tags) : "";

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

            var selectedTagIds = news.Tags?.Split(',').ToList() ?? new List<string>();
            ViewBag.Tags = new MultiSelectList(await _context.Tags.ToListAsync(), "Title", "Title", selectedTagIds);
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "FullName", news.UserId);

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

/// <summary>
/// Handles asynchronous image uploads specifically for the CKEditor 5 WYSIWYG editor.
/// This endpoint receives a file via a POST request, saves it to a designated folder
/// within the web root (wwwroot), and returns a JSON object containing the public URL
/// of the uploaded file, which CKEditor then uses to display the image.
/// </summary>
/// <param name="upload">
/// The uploaded file, sent from the client-side CKEditor upload adapter.
/// The parameter name 'upload' must match the key used in the FormData object on the client side.
/// </param>
/// <returns>
/// On success, an JsonResult with a 'url' property (e.g., { "url": "/images/news/some-guid.jpg" }).
/// On failure (e.g., no file uploaded), a BadRequest result.
/// </returns>
[HttpPost]
// Disables the anti-forgery token validation for this specific action.
// This is often necessary for AJAX-based file uploaders like CKEditor's, which may not
// automatically send the token. Use with caution and consider alternative security measures if needed.
[IgnoreAntiforgeryToken]
public async Task<IActionResult> UploadImage(IFormFile upload)
{
    // 1. VALIDATION: Check if a file was actually sent and has content.
    if (upload?.Length > 0)
    {
        // 2. FILE NAMING: Generate a unique filename to prevent conflicts and overwriting existing files.
        // Using a GUID is a standard practice for ensuring uniqueness.
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(upload.FileName)}";

        // 3. PATH CONSTRUCTION: Define the storage path for the image.
        // The relative path within the 'wwwroot' directory. This keeps uploads organized.
        var folderPath = Path.Combine("images", "news");
        // The absolute physical path on the server where the file will be saved.
        var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderPath);

        // 4. DIRECTORY CREATION: Ensure the target directory exists. If not, create it.
        // This prevents exceptions if the folder structure is not already in place.
        Directory.CreateDirectory(saveDir);

        // 5. FILE SAVING: Create the full path for the new file and save it to the disk.
        var filePath = Path.Combine(saveDir, fileName);
        // Use an async file stream to write the file. 'await using' ensures proper disposal.
        await using var stream = new FileStream(filePath, FileMode.Create);
        // Copy the contents of the uploaded file to the destination stream.
        await upload.CopyToAsync(stream);

        // 6. RESPONSE: Construct the public URL that the browser will use to access the image.
        // It's crucial that this path is relative to the web root.
        // Replace backslashes with forward slashes for URL compatibility.
        var url = $"/{folderPath.Replace("\\", "/")}/{fileName}";

        // Return a JSON object in the format expected by the CKEditor upload adapter.
        // The client-side script is specifically looking for this 'url' property.
        return Json(new { url });
    }

    // If no file was provided in the request, return an HTTP 400 Bad Request response.
    return BadRequest(new { message = "No file uploaded or file is empty." });
}

    }
}
