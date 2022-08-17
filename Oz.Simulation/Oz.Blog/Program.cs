using Oz.Blog.Contracts;
using Oz.Blog.Middlewares;
using Oz.Blog.Services;
using Oz.Blog.Settings;
using Oz.Data.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OzDataOptions>(builder.Configuration.GetSection("OzDataOptions"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IBlogDataService, OzBlogDataService>();

builder.Services.AddScoped<AuthMiddleware>();
builder.Services.AddScoped<IAdminService, AdminService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthMiddleware();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();