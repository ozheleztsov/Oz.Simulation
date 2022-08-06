using Oz.Snake.Contracts;
using Oz.Snake.HostedServices;
using Oz.Snake.Hubs;
using Oz.Snake.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<ISnakeService, SnakeService>();
builder.Services.AddScoped<IOutOfHubAccessor, OutOfHubAccessor>();
builder.Services.AddHostedService<FoodFiller>();

var app = builder.Build();

app.MapHub<SnakeHub>("/snake");

app.Run();