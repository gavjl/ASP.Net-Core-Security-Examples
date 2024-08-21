using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRateLimiter(_ => _
    .AddPolicy("twoPerMinuteRateLimiter", context =>
        {
            string key = string.Empty;
            // NOTE: X-Forwarded-For is often used to get the IP address when behind proxies etc
            // Make sure the client can't fake these headers and they're only set by the proxy
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var headerValues))
            {
                key = headerValues.FirstOrDefault();
            }
            else
            {
                //If you are using some sort of proxy then this may just contain the address of the proxy every time. Not what you want.
                key = context.Connection.RemoteIpAddress.ToString();
            }

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: key,
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 2
                });
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

//IRatelimitedPolicy = new IRateLimiterPolicy<FixedWindowRateLimiter>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    //.RequireRateLimiting("twoPerMinuteRateLimiter");

app.Run();
