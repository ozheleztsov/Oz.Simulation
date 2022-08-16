using Azure;
using Azure.Data.Tables;

const string TableName = "Test";
const string ConnectionString = "";
var tableServiceClient = new TableServiceClient(ConnectionString);
var tableClient = tableServiceClient.GetTableClient(TableName);

try
{
    await tableClient.CreateIfNotExistsAsync();
}
catch (Exception exception)
{
    Console.WriteLine(exception);
}

var id = Guid.NewGuid();

var tableEntity = new TableEntity()
{
    PartitionKey = "olehz", RowKey = id.ToString(), Id = id,
    CreateDate = DateTime.UtcNow,
    ModifiedDate = DateTime.UtcNow
};
await tableClient.AddEntityAsync(tableEntity);
Console.WriteLine("Done");

public class TableEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public List<ListElement> ListElements { get; set; } = new();
}

public sealed class ListElement
{
    public DateTime CreateDate { get; set; }
    public string Content { get; set; } = string.Empty;
}