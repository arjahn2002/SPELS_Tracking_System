using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SPELS_TRACKING_SYSTEM.Data;
using SPELS_TRACKING_SYSTEM.Hubs;
using SPELS_TRACKING_SYSTEM.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<SPELS_TRACKING_SYSTEMContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SPELS_TRACKING_SYSTEMContext")
        ?? throw new InvalidOperationException("Connection string 'SPELS_TRACKING_SYSTEMContext' not found.")
    ));

// Session and caching
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);
    options.Cookie.Name = ".SPELS_TRACKING_SYSTEM.Session";
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
});

// Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });

// SignalR with detailed errors
builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PermissionService>();

var app = builder.Build();

// Exception handler
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Map routes and hubs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
