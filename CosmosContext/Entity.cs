using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace TestDatabaseDbTrigger.CosmosContext
{
    public static class Entity
    {
        public static JObject AsEntity(dynamic item = null, string id = null, string entityType = null, string partitionKey = null)
        {
            var source = new JObject();

            if (item is not null)
            {

                foreach (PropertyInfo prop in item.GetType().GetProperties())
                {
                    source[prop.Name] = JToken.FromObject(prop.GetValue(item));
                }
            }

            var EntityType = entityType = entityType.ToTitleCase();

            source[nameof(id)] = id;
            source[nameof(EntityType)] = EntityType;
            source["PartitionKey"] = partitionKey ?? $"{EntityType}_{DateTime.Now.Ticks}_{new Random().Next(100)}";
            return source;
        }
    }

    public static class StringExt
    {
        public static string ToTitleCase(this string source)
        {
            return string.Join("", source.Select((e,i) => e=i>0?e:$"{e}".ToUpper()[0]));
        }
    }
}
