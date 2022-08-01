using Oz.Snake.Hubs;
using Oz.Snake.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<ISnakeService, SnakeService>();

var app = builder.Build();

app.MapHub<SnakeHub>("/snake");

app.Run();