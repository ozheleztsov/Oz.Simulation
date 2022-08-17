using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Oz.Blog.Entities;
using Oz.Blog.Settings;
using Oz.Data.Contracts;
using System.Linq.Expressions;

namespace Oz.Blog.Services;

public class OzBlogDataService : IBlogDataService
{
    private readonly ILogger<OzBlogDataService> _logger;
    private readonly TableClient _tableClient;

    public OzBlogDataService(ILogger<OzBlogDataService> logger, IOptions<OzDataOptions> options)
    {
        _logger = logger;
        var tableServiceClient = new TableServiceClient(options.Value.StorageAccountConnectionString);
        _tableClient = tableServiceClient.GetTableClient(options.Value.BlogTableName);
    }


    public async Task CreateBlogEntryAsync(BlogEntryCreateData createData, CancellationToken cancellationToken = default)
    {
        var blogEntry = BlogEntry.Create(createData);
        await CreateBlogTableIfNotExists(cancellationToken).ConfigureAwait(false);
        await _tableClient.AddEntityAsync(blogEntry, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BlogEntryResponseData?> GetBlogEntryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blogEntry = await GetBlogEntryByIdInnerAsync(id, cancellationToken).ConfigureAwait(false);
        return blogEntry?.GetResponseData();
    }


    public async Task<IEnumerable<BlogEntryResponseData>> GetBlogEntriesAsync(Expression<Func<BlogEntry, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await CreateBlogTableIfNotExists(cancellationToken).ConfigureAwait(false);
        List<BlogEntry> blogEntries = new();
        await foreach (var blogEntry in _tableClient.QueryAsync<BlogEntry>(predicate, cancellationToken: cancellationToken))
        {
            blogEntries.Add(blogEntry);
        }

        return blogEntries.Select(x => x.GetResponseData());
    }

    public async Task UpdateBlogEntryAsync(Guid id, BlogEntryUpdateData updateData, CancellationToken cancellationToken = default)
    {
        var blogEntry = await GetBlogEntryByIdInnerAsync(id, cancellationToken).ConfigureAwait(false);
        blogEntry?.Update(updateData);
        if (blogEntry != null)
        {
            await _tableClient.UpdateEntityAsync(blogEntry, ETag.All, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DeleteBlogEntryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blogEntry = await GetBlogEntryByIdInnerAsync(id, cancellationToken).ConfigureAwait(false);
        if (blogEntry != null)
        {
            await _tableClient.DeleteEntityAsync(blogEntry.PartitionKey, blogEntry.RowKey, ETag.All, cancellationToken);
        }
    }

    private async Task CreateBlogTableIfNotExists(CancellationToken cancellationToken)
    {
        try
        {
            await _tableClient.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException exception)
        {
            _logger.LogInformation("Create azure table message: {Message}", exception.Message);
        }
    }

    private async Task<BlogEntry?> GetBlogEntryByIdInnerAsync(Guid id, CancellationToken cancellationToken)
    {
        await CreateBlogTableIfNotExists(cancellationToken).ConfigureAwait(false);
        List<BlogEntry> blogEntries = new();
        await foreach (var blogEntry in _tableClient.QueryAsync<BlogEntry>(x => x.Id == id, cancellationToken: cancellationToken))
        {
            blogEntries.Add(blogEntry);
        }

        return !blogEntries.Any() ? null : blogEntries.FirstOrDefault();
    }
}