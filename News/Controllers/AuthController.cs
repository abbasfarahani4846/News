using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

using News.Models.ViewModels;

using System.Security.Claims;
using News.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace News.Controllers
{
    public class AuthController : Controller
    {
        private readonly NewsContext _context; // Replace with your DbContext name

        public AuthController(NewsContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // If the user is already logged in, redirect them to the home page.
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Find the user by username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user != null && user.Password == model.Password)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Your account is disabled.");
                        return View(model);
                    }

                    // --- Create Claims ---
                    // Claims are pieces of information about the user.
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim("FullName", user.FullName),
                        // Add IsAdmin as a Role claim. This is crucial for authorization.
                        new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                    };

                    // If user.IsAdmin is true, we can add another specific claim if needed.
                    if (user.IsAdmin)
                    {
                        claims.Add(new Claim("AdminSpecificPermission", "CanDeleteUsers")); // Example
                    }

                    // --- Create Identity & Principal ---
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        // Allows the cookie to persist across browser sessions if "Remember Me" is checked.
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : (DateTimeOffset?)null
                    };

                    // --- Sign In ---
                    // This creates the encrypted authentication cookie and adds it to the response.
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Redirect to the originally requested URL, or to the admin dashboard if none was specified.
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                   
                        if (user.IsAdmin)
                        {
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        }
                        else
                        {
                            return RedirectToAction("Index", "News", new { area = "Admin" });
                        }
                    }
                }

                // If login fails, add a generic error message.
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }

            // If model state is invalid, return the view with the validation messages.
            return View(model);
        }

        // GET: /Auth/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // This clears the authentication cookie.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
