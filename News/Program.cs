using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using News.Models.Db;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- START: Authentication Configuration ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Sets the path for the login page. 
        // If an unauthorized user tries to access a protected page, they will be redirected here.
        options.LoginPath = "/Auth/Login";

        // Sets the path for the access denied page.
        options.AccessDeniedPath = "/Auth/Login"; // Optional: Create a view for this.

        // Sets the expiration time for the cookie.
        options.ExpireTimeSpan = TimeSpan.FromDays(10);

        // Makes the cookie essential for the application to function correctly.
        options.SlidingExpiration = true;
    });
// --- END: Authentication Configuration ---


builder.Services.AddDbContext<NewsContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()   // به همه دامنه‌ها اجازه می‌دهد
                  .AllowAnyMethod()  // به همه متدهای HTTP اجازه می‌دهد (GET, POST, etc)
                  .AllowAnyHeader(); // به همه هدرها اجازه می‌دهد
        });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// --- IMPORTANT: Add Authentication and Authorization middleware ---
// These must be placed between UseRouting() and MapControllerRoute().
app.UseAuthentication(); // This middleware identifies the user based on the cookie.
app.UseAuthorization();  // This middleware checks if the identified user has permission to access a resource.

app.MapStaticAssets();

app.UseCors("AllowAll");

app.MapControllerRoute(
  name: "areas",
  pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();



app.Run();
