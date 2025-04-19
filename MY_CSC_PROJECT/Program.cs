using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MY_CSC_PROJECT.Data;
using MY_CSC_PROJECT.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MY_CSC_PROJECTContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MY_CSC_PROJECTContext") ?? throw new InvalidOperationException("Connection string 'MY_CSC_PROJECTContext' not found.")));

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".MY_CSC_PROJECT.Session";
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
