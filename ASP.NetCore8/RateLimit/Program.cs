using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "twoPerMinuteRateLimiter", options =>
    {
        options.Window = TimeSpan.FromMinutes(1);
        options.PermitLimit = 2;
    })
    .OnRejected = (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.WriteAsync("Woah, these calls are expensive you know! No more than 2 in a minute please!");
        return ValueTask.CompletedTask;
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
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    //.RequireRateLimiting("twoPerMinuteRateLimiter");

app.Run();
