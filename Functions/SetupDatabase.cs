using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;

using CosmosContext;
using System;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Generic;
using TestDatabaseDbTrigger.CosmosContext;
using Newtonsoft.Json.Linq;

namespace TestDatabaseDbTrigger.Functions;

public partial class SetupDatabase
{
    public const string FunctionSupername = nameof(SetupDatabase);

    private readonly DbContextA contextA;
    private readonly ILogger<SetupDatabase> _logger;

    public SetupDatabase(ILogger<SetupDatabase> logger, CosmosContextFactory contextFactory)
    {
        contextA = contextFactory.GetContext<DbContextA>();
        _logger = logger;
    }

    [FunctionName(FunctionSupername)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
    {
        await SetupContainers();

        return new OkObjectResult($"Hello");
    }

    public async Task SetupContainers()
    {
        await Task.WhenAll(
            CreateAirportsContainer(),
            CreateVehiclesContainer(),
            CreatePassengersContainer()
        );
    }

    public async Task CreateAirportsContainer()
    {
        var container = contextA.AirportContainer;

        dynamic CreateDocument(string itemLabel) => 
            Entity.AsEntity(
                new
                {
                    Address = "112 E SOME ST, City, State, -77777"
                },
                id: itemLabel,
                entityType: "Airport"
            );

        var items = "OK,KS,NY,TX,OR".Split(",").Select((e, i) => $"{e} {(i % 2 == 0 ? "intl " : "")}Airport");

        using var setIterator = container.GetItemLinqQueryable<Document>()
            .Where(e => items.Contains(e.Id))
            .Select(e => e.Id)
            .ToFeedIterator();

        var diff = items.Except(setIterator.Result()).ToArray();

        foreach (var item in diff)
        {
            var insert = CreateDocument(item);
            await container.CreateItemAsync(insert);
        }

        if (!diff.Any()) return;
        Console.WriteLine($"Created new items {string.Join(", ", diff)}");
    }

    public async Task CreateVehiclesContainer()
    {
        var container = contextA.VehicleContainer;
        var entityType = "Vehicle";
        var vehicleType = "Aircraft";

        JObject CreateDocument(string itemLabel) => 
            Entity.AsEntity(
            new
            {
                VehicleType = "Aircraft",
                PassengerCapacity = 300,
                RequiredPilots = 2,
                RequiredAttendants = 3,
                CargoKgMax = 4000,
                KmPerLitreEfficiency = 5,
            },
            id: itemLabel,
            entityType: entityType,
            partitionKey: $"{entityType}_{vehicleType}_{itemLabel}"
        );

        var items = "1483,1662,9201,4094".Split(",");

        using var setIterator = container.GetItemLinqQueryable<Document>()
            .Where(e => items.Contains(e.Id))
            .Select(e => e.Id)
            .ToFeedIterator();

        var diff = items.Except(setIterator.Result()).ToArray();

        foreach (var item in diff)
        {
            var doc = CreateDocument(item);
            await container.CreateItemAsync(doc);
        }

        if (!diff.Any()) return;
        Console.WriteLine($"Created new items {string.Join(", ", diff)}");
    }

    public async Task CreatePassengersContainer()
    {
        var container = contextA.PassengerContainer;
        var entityType = "Passenger";
        dynamic CreateDocument(string itemLabel) =>
            Entity.AsEntity(
                id: itemLabel,
                entityType: entityType,
                partitionKey: entityType
            );

        var items = Enumerable.Range(0,11).Select(num=>$"Passenger_{num}");

        using var setIterator = container.GetItemLinqQueryable<Document>()
            .Where(e => items.Contains(e.Id))
            .Select(e => e.Id)
            .ToFeedIterator();

        var diff = items.Except(setIterator.Result()).ToArray();

        foreach (var item in diff)
        {
            await container.CreateItemAsync(CreateDocument(item));
        }

        if (!diff.Any()) return;
        Console.WriteLine($"Created new items {string.Join(", ", diff)}");
    }

    public void CheckTest()
    {

    }
}

