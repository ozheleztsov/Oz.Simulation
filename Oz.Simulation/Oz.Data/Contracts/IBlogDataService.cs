using Oz.Data.Entities;

namespace Oz.Data.Contracts;

public interface IBlogDataService
{
    Task CreateBlogEntryAsync(BlogEntryCreateData createData, CancellationToken cancellationToken = default);
    Task<BlogEntryResponseData?> GetBlogEntryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogEntryResponseData>> GetBlogEntriesAsync(Func<BlogEntry, bool> predicate, CancellationToken cancellationToken = default);
    Task UpdateBlogEntryAsync(Guid id, BlogEntryUpdateData updateData, CancellationToken cancellationToken = default);
    Task DeleteBlogEntryAsync(Guid id, CancellationToken cancellationToken = default);
}