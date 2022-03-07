using CosmosContext;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: FunctionsStartup(typeof(TestDatabaseDbTrigger.Startup))]

namespace TestDatabaseDbTrigger;

public class Startup : FunctionsStartup
{
    public const string COSMOS_DB_URI = nameof(COSMOS_DB_URI);
    public const string COSMOS_PRIMARY_KEY = nameof(COSMOS_PRIMARY_KEY);
    public const string StringSetting = nameof(StringSetting);

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<CosmosContextFactory>();
    }
}
