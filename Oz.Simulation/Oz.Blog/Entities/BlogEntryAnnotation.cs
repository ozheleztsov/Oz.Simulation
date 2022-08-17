// ReSharper disable ClassNeverInstantiated.Global

namespace Oz.Blog.Entities;

public sealed class BlogEntryAnnotation
{
    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public string Content { get; set; } = string.Empty;
}