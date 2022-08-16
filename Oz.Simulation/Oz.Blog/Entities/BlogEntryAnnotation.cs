// ReSharper disable ClassNeverInstantiated.Global

namespace Oz.Data.Entities;

public sealed class BlogEntryAnnotation
{
    public DateTime CreateDate { get; set; }
    public string Content { get; set; } = string.Empty;
}