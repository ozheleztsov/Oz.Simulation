// ReSharper disable ClassNeverInstantiated.Global

namespace Oz.Data.Entities;

public sealed record BlogEntryCreateData(
    string Title, string Content, string AuthorSign, List<BlogEntryAnnotation> Annotations);