using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace CosmosContext;

public interface IContext
{
    public DatabaseOptions DatabaseOptions { get; }
    public Database Database { get; }
    public Dictionary<string, Database> DatabaseMap { get; set; }
}
