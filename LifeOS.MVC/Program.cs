using LifeOS.MVC.Data;
using LifeOS.MVC.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options => { options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()); options.Filters.Add(new LifeOS.MVC.Filters.ValidateFormFilter()); });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Services
builder.Services.AddScoped<LifeOS.MVC.Services.ProductivityService>();
builder.Services.AddScoped<LifeOS.MVC.Services.StreakService>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/lifeos-health", () => Results.Json(new { application = "LifeOS", status = "ready" }));

app.Run();


