using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Oz.Data.Settings;

namespace Oz.Blog.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IOptions<OzDataOptions> _options;

    public IndexModel(IOptions<OzDataOptions> options, ILogger<IndexModel> logger)
    {
        _options = options;
        _logger = logger;
    }

    public string StorageAccountConnectionString { get; set; } = string.Empty;
    public string BlogTableName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        StorageAccountConnectionString = _options.Value.StorageAccountConnectionString ?? string.Empty;
        BlogTableName = _options.Value.BlogTableName ?? string.Empty;
        await Task.CompletedTask;
    }
}