using System.Collections.Generic;

namespace CosmosContext;

public class CosmosContextFactory
{

    public Dictionary<string, Context> ContextMap { get; set; } = new();

    public T GetContext<T>() where T : Context, new()
    {
        Context currentContext = new T();

        if (ContextMap.TryAdd(currentContext.Database.Id, currentContext) || ContextMap.TryGetValue(currentContext.Database.Id, out currentContext)) { }

        return currentContext as T;
    }
}
