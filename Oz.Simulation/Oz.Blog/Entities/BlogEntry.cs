using Azure;
using Azure.Data.Tables;
using System.Text.Json;

// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Oz.Data.Entities;

public class BlogEntry : ITableEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string AuthorSign { get; set; } = string.Empty;
    public string Annotations { get; set; } = string.Empty;
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public static BlogEntry Create(BlogEntryCreateData createData)
    {
        var id = Guid.NewGuid();

        return new BlogEntry
        {
            RowKey = id.ToString(),
            PartitionKey = createData.AuthorSign,
            Id = id,
            Title = createData.Title,
            Content = createData.Content,
            CreateDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            AuthorSign = createData.AuthorSign,
            Annotations = JsonSerializer.Serialize(createData.Annotations.Select(a => a).ToList(),
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
        };
    }

    public BlogEntryResponseData GetResponseData()
    {
        var annotationsList = JsonSerializer.Deserialize<List<BlogEntryAnnotation>>(Annotations,
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new List<BlogEntryAnnotation>();
        
        return new(Id,
            Title, Content, CreateDate, ModifiedDate, AuthorSign, annotationsList);
    }

    public void Update(BlogEntryUpdateData updateData)
    {
        Title = updateData.Title;
        Content = updateData.Content;
        AuthorSign = updateData.AuthorSign;
        Annotations = JsonSerializer.Serialize(updateData.Annotations,
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}