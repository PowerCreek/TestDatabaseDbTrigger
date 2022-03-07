using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CosmosContext;

public class Context : IContext
{
    public virtual DatabaseOptions DatabaseOptions { get; protected set; }
    public Dictionary<string, Container> ContainerMap { get; set; }
    public Dictionary<string, Database> DatabaseMap { get; set; }


    public CosmosClient CosmosClient { get; set; }

    public string DatabaseName { get => DatabaseOptions.DatabaseName; }
    public Database Database
    {
        get
        {
            PopulateDatabaseMap();
            return FetchDatabase();
        }
    }

    private Database FetchDatabase() => DatabaseMap[DatabaseName] = DatabaseMap.TryGetValue(DatabaseName, out var database) switch
    {
        bool exists when exists && database is null => CosmosClient.GetDatabase(DatabaseName),
        bool exists when exists => database,
        _ when DatabaseOptions.CreateIfNotExists => CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName).GetAwaiter().GetResult(),
        _ => throw new KeyNotFoundException($"Databasename {DatabaseName} not found")
    };

    private void PopulateDatabaseMap()
    {
        if (DatabaseMap != null) return;
        DatabaseMap = new();

        async Task CheckForDatabase()
        {
            using var iterator = CosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();

            while (iterator.HasMoreResults)
            {
                foreach (DatabaseProperties db in await iterator.ReadNextAsync())
                {
                    lock (DatabaseMap)
                    {
                        DatabaseMap[db.Id] = null;
                    }
                }
            }
        }

        CheckForDatabase().GetAwaiter().GetResult();
    }

    protected Container GetContainer(ContainerOptions options)
    {
        if (ContainerMap is null)
        {
            ContainerMap = new ();

            var containerIterator = Database.GetContainerQueryIterator<ContainerProperties>();
            var containers = containerIterator.ReadNextAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (var containerItem in containers)
            {
                this.ContainerMap[containerItem.Id] = null;
            }
        }

        return ContainerMap[options.ContainerName] = ContainerExistsOrThrow(options);
    }

    private Container ContainerExistsOrThrow(ContainerOptions options) => 
        ContainerMap.TryGetValue(options.ContainerName, out var container) switch
    {
        //Container exists, but it's not in the map.
        //Key exists in the map, but the value is null.
        _ when container is not null => container,

        //key exists
        true => Database.GetContainer(options.ContainerName),

        //Key does not exist, but create the database
        bool exists when !exists && options.CreateIfNotExists =>
            Database.CreateContainerIfNotExistsAsync(options.ContainerName, options.PartitionKey).GetAwaiter().GetResult().Container,

        _ => throw new KeyNotFoundException($"Could not locate {options.ContainerName}")
    };

}

public class DatabaseOptions
{
    public bool CreateIfNotExists { get; set;}
    public string DatabaseName { get; set;}
}

public class ContainerOptions
{
    public bool CreateIfNotExists { get; init; }
    public string DatabaseName { get; set; }
    public string ContainerName { get; set; }
    public string PartitionKey { get; init; }
}

public static class FeedIteratorExt
{

    public static IEnumerable<T> Result<T>(this FeedIterator<T> feedIterator)
    {
        foreach (var item in feedIterator.ReadNextAsync().GetAwaiter().GetResult())
        {
            yield return item;
        }
    }
}