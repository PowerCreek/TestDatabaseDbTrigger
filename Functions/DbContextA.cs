
using CosmosContext;
using Microsoft.Azure.Cosmos;
using System.Configuration;
using System;
using Microsoft.Extensions.Logging;

using static TestDatabaseDbTrigger.Startup;

namespace TestDatabaseDbTrigger.Functions;

public partial class SetupDatabase
{
    public class DbContextA : Context
    {
        public const string DATABASE_NAME = "projectMain";
        public const string COLLECTION_AIRPORTS = "Airports";
        public const string COLLECTION_VEHICLES = "Vehicles";
        public const string AIRPORT_PASSENGERS = "LightWriteData";

        public static readonly ContainerOptions ContainerOptions_Vehicles = new()
        {
            DatabaseName = DATABASE_NAME,
            ContainerName = "LightWriteData",
            PartitionKey = "/PartitionKey",
            CreateIfNotExists = true,
        };

        public static ContainerOptions ContainerOptions_Airports = new()
        {
            DatabaseName = DATABASE_NAME,
            ContainerName = "LightWriteData",
            PartitionKey = "/PartitionKey",
            CreateIfNotExists = true,
        };

        public static ContainerOptions ContainerOptions_Passengers = new()
        {
            DatabaseName = DATABASE_NAME,
            ContainerName = "LightWriteData",
            PartitionKey = "/PartitionKey",
            CreateIfNotExists = true,
        };

        public override DatabaseOptions DatabaseOptions 
        { 
            get => base.DatabaseOptions??=new DatabaseOptions 
            { 
                DatabaseName = DATABASE_NAME,
            };
        }

        public Container VehicleContainer => GetContainer(ContainerOptions_Vehicles);
        public Container AirportContainer => GetContainer(ContainerOptions_Airports);
        public Container PassengerContainer => GetContainer(ContainerOptions_Passengers);

        public Container GetContainerFromOptions(ContainerOptions options) => GetContainer(options);

        public DbContextA()
        {
            var uri = ConfigurationManager.AppSettings[COSMOS_DB_URI] ?? Environment.GetEnvironmentVariable(COSMOS_DB_URI);
            var key = ConfigurationManager.AppSettings[COSMOS_PRIMARY_KEY] ?? Environment.GetEnvironmentVariable(COSMOS_PRIMARY_KEY);

            CosmosClient = new CosmosClient(uri, key);
        }
    }
}
