using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using static TestDatabaseDbTrigger.Functions.SetupDatabase;
using CosmosContext;
using Microsoft.Azure.Cosmos;
using TestDatabaseDbTrigger.CosmosContext;

namespace TestDatabaseDbTrigger.Functions;

public class DbContextATrigger
{
    public const string FunctionSupername = $"{nameof(SetupDatabase.DbContextA.AirportContainer)}_DbTrigger";

    public readonly DbContextA contextA;

    public readonly ContainerOptions PassengerDataOptions = new ()
    {
        DatabaseName =  DbContextA.DATABASE_NAME,
        ContainerName = "LightWriteData",
        PartitionKey = "/EntityType",
        CreateIfNotExists = true,
    };

    public DbContextATrigger(CosmosContextFactory contextFactory)
    {
        contextA = contextFactory.GetContext<DbContextA>();
    }

    [FunctionName(FunctionSupername)]
    public async Task PassengerFunction(
        [CosmosDBTrigger(
            databaseName: SetupDatabase.DbContextA.DATABASE_NAME,
            collectionName: SetupDatabase.DbContextA.AIRPORT_PASSENGERS,
            ConnectionStringSetting = "StringSetting",
            LeaseCollectionPrefix = FunctionSupername,
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> changedDocuments
        )
    {
        foreach(var document in changedDocuments)
        {
            CascadeToPartition(document);
        }
    }

    public void CascadeToPartition(Document doc)
    {
        dynamic item = doc;
        Console.WriteLine(item);

        //Container partition = PassengerSpecificContainer(item.id);

        var activity =
            Activity.CreateActivity(
                new Activity
                {
                    ActionData = new 
                    { 
                        ActionType = "PassengerCreated",
                        ActionDate = DateTime.UtcNow
                    },
                    SomeData_1 = "PassengerData1",
                    SomeData_3 = "PassengerData2"
                },
                item.id
            );

        //partition.CreateItemAsync(activity);
    }

    public class Activity
    {
        public const string ENTITY_TYPE = "Activity";
        
        public object ActionData { get; set; }
        public string SomeData_1 { get; set; } = "none";
        public string SomeData_2 { get; set; } = "none";
        public string SomeData_3 { get; set; } = "none";

        public static dynamic CreateActivity(dynamic source, string id)
        {
            return Entity.AsEntity(
                source,
                id: $"{ENTITY_TYPE}_{id}",
                entityType: ENTITY_TYPE
            );
        }
    }

    public Container PassengerSpecificContainer(string key) => contextA.GetContainerFromOptions(
        new() 
        {
            DatabaseName = DbContextA.DATABASE_NAME,
            ContainerName = key,
            PartitionKey = "/id",
            CreateIfNotExists = true,
        }
    );
}
