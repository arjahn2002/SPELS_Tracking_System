using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SPELS_TRACKING_SYSTEM.Data;
using SPELS_TRACKING_SYSTEM.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SPELS_TRACKING_SYSTEMContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SPELS_TRACKING_SYSTEMContext") ?? throw new InvalidOperationException("Connection string 'SPELS_TRACKING_SYSTEMContext' not found.")));

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".SPELS_TRACKING_SYSTEM.Session";
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PermissionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
