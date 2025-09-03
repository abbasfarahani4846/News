using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;

namespace News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenusController : Controller
    {
        // Private field to hold the database context.
        private readonly NewsContext _context;

        // Constructor to initialize the database context via Dependency Injection.
        public MenusController(NewsContext context)
        {
            _context = context;
        }

        // GET: Admin/Menus or Admin/Menus?id=5
        // This action handles displaying both top-level menus and sub-menus.
        public async Task<IActionResult> Index(int? id)
        {
            // If an 'id' is provided, it means we are viewing sub-menus.
            if (id != null)
            {
                // Find the parent menu item to display its details (e.g., its title).
                var parent = await _context.Menus.FirstOrDefaultAsync(x => x.Id == id);
                if (parent == null)
                {
                    // If the parent menu doesn't exist, return a 404 Not Found error.
                    return NotFound();
                }

                // Pass the parent menu object to the view to display its title as a header.
                ViewBag.Parent = parent;
                // Return the view with a list of all menus that have this parent ID.
                return View(await _context.Menus.Where(x => x.ParentId == id).ToListAsync());
            }

            // If no 'id' is provided, display the top-level (root) menus.
            // These are menus where ParentId is null.
            return View(await _context.Menus.Where(x => x.ParentId == null).ToListAsync());
        }


        // GET: Admin/Menus/Create or Admin/Menus/Create?parentId=5
        // This action displays the form for creating a new menu item.
        public IActionResult Create(int? parentId)
        {
            // Pass the parentId to the view so it can be set in a hidden field.
            // This ensures the new menu is created as a child of the correct parent.
            ViewBag.parentid = parentId;

            return View();
        }

        // POST: Admin/Menus/Create
        // This action handles the form submission for creating a new menu.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Link,ParentId,Position,Priority")] Menu menu)
        {
            // Check if the submitted data is valid based on the model's validation attributes.
            if (ModelState.IsValid)
            {
                // Add the new menu object to the context and save changes to the database.
                _context.Add(menu);
                await _context.SaveChangesAsync();

                // If the new menu is a sub-menu (has a ParentId),
                // redirect back to the index page for that parent.
                if (menu.ParentId != null)
                {
                    return Redirect("/admin/menus/index/" + menu.ParentId);
                }

                // If it's a top-level menu, redirect to the main index page.
                return RedirectToAction(nameof(Index));
            }

            // If the model is not valid, pass the parentId back to the view
            // and redisplay the form with the validation errors.
            ViewBag.parentid = menu.ParentId;
            return View(menu);
        }

        // GET: Admin/Menus/Edit/5
        // Displays the form for editing an existing menu item.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the menu item in the database by its ID.
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            // Return the view with the found menu item.
            return View(menu);
        }

        // POST: Admin/Menus/Edit/5
        // Handles the form submission for updating a menu item.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Link,ParentId,Position,Priority")] Menu menu)
        {
            // Ensure the ID from the URL matches the ID from the submitted form data.
            if (id != menu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mark the entity as modified and save the changes.
                    _context.Update(menu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency errors (e.g., if another user deleted the record while you were editing it).
                    if (!MenuExists(menu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // If the updated menu is a sub-menu, redirect back to its parent's index page.
                if (menu.ParentId != null)
                {
                    return Redirect("/admin/menus/index/" + menu.ParentId);
                }

                // If it's a top-level menu, redirect to the main index page.
                return RedirectToAction(nameof(Index));
            }

            // If the model is not valid, redisplay the form with the submitted data and validation errors.
            return View(menu);
        }

        // GET: Admin/Menus/Delete/5
        // Displays the confirmation page for deleting a menu item.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the menu item to be deleted.
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menu == null)
            {
                return NotFound();
            }

            // Return the view with the menu item's details.
            return View(menu);
        }

        // POST: Admin/Menus/Delete/5
        // This action is executed when the user confirms the deletion.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the menu item by ID.
            var menu = await _context.Menus.FindAsync(id);
            if (menu != null)
            {
                // Remove the menu item from the context.
                _context.Menus.Remove(menu);
            }

            // Save the changes to the database.
            await _context.SaveChangesAsync();

            // If the deleted menu is a sub-menu, redirect back to its parent's index page.
            if (menu.ParentId != null)
            {
                return Redirect("/admin/menus/index/" + menu.ParentId);
            }

            // Redirect to the index page.
            return RedirectToAction(nameof(Index));
        }

        // Private helper method to check if a menu item exists in the database.
        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.Id == id);
        }
    }
}