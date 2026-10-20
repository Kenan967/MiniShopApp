using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("orderLimiter", options =>
    {
        options.PermitLimit = 5;

        options.Window =
            TimeSpan.FromSeconds(30);

        options.QueueLimit = 0;
        //options.QueueLimit = 3;

        options.QueueProcessingOrder =
            QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

app.UseCors("ReactPolicy");

//app.Use(async (context, next) =>
//{
//    if (context.Request.Method == HttpMethods.Get &&
//        context.Request.Path.StartsWithSegments("/api/products"))
//    {
//        context.Response.StatusCode =
//            StatusCodes.Status403Forbidden;

//        await context.Response.WriteAsJsonAsync(new
//        {
//            message = "Getting products is not allowed through API Gateway."
//        });

//        return;
//    }

//    await next();
//});

app.UseRateLimiter();

app.MapReverseProxy();

app.Run();