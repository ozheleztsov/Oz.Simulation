// ReSharper disable NotAccessedPositionalProperty.Global

namespace Oz.Data.Entities;

public sealed record BlogEntryResponseData(
    Guid Id, string Title, string Content, DateTime CreateDate, DateTime ModifiedDate,
    string AuthorSign, List<BlogEntryAnnotation> Annotations);