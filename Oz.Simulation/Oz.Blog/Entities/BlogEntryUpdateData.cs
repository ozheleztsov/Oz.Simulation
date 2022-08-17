// ReSharper disable ClassNeverInstantiated.Global

namespace Oz.Blog.Entities;

public sealed record BlogEntryUpdateData(string Title, string Content, string AuthorSign, List<BlogEntryAnnotation> Annotations);