using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using News.Models.Db;

namespace News.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly NewsContext _context;

        public NewsletterController(NewsContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Subscribe(string email)
        {
            // Server-side validation for email format
            if (string.IsNullOrEmpty(email) || !System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return Json(new { success = false, message = "Please enter a valid email address." });
            }

            // Check for duplicate email
            var isAlreadySubscribed = await _context.Subscribers.AnyAsync(s => s.Email == email.ToLower());
            if (isAlreadySubscribed)
            {
                return Json(new { success = false, message = "This email address is already subscribed!" });
            }

            // Create a new subscriber object
            var subscriber = new Subscriber
            {
                Email = email.ToLower(),
                SubscribedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Add to database and save
            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            // Return a success response
            return Json(new { success = true, message = "Thank you for subscribing!" });
        }
    }
}
