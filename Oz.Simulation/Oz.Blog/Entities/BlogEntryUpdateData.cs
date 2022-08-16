// ReSharper disable ClassNeverInstantiated.Global

namespace Oz.Data.Entities;

public sealed record BlogEntryUpdateData(string Title, string Content, string AuthorSign, List<BlogEntryAnnotation> Annotations);