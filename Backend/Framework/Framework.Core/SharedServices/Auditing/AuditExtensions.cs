namespace Framework.Core.SharedServices.Auditing
{
    // public static class AuditExtensions
    // {
    //     private static JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings()
    //     {
    //         ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    //         ContractResolver = new CamelCasePropertyNamesContractResolver(),
    //         Formatting = Formatting.None,
    //         NullValueHandling = NullValueHandling.Ignore
    //     };

    //     private static JsonSerializer JsonSerializer { get; } = JsonSerializer.Create(JsonSerializerSettings);

    //     public static T AutoAudit<T>(this EntityEntry entry, Func<T> createAuditFactory)
    //where T : Audit
    //     {
    //         var history = createAuditFactory();
    //         history.TableName = entry.Metadata.GetTableName();

    //         // Get the mapped properties for the entity type.
    //         // (include shadow properties, not include navigations & references)
    //         var properties = entry.Properties;

    //         var formatting = JsonSerializerSettings.Formatting;
    //         var jsonSerializer = JsonSerializer;
    //         var json = new JObject();

    //         switch (entry.State)
    //         {
    //             case EntityState.Added:
    //                 foreach (var prop in properties)
    //                 {
    //                     if (prop.Metadata.IsKey() || prop.Metadata.IsForeignKey())
    //                     {
    //                         continue;
    //                     }
    //                     json[prop.Metadata.Name] = prop.CurrentValue != null
    //                         ? JToken.FromObject(prop.CurrentValue, jsonSerializer)
    //                         : JValue.CreateNull();
    //                 }

    //                 // REVIEW: what's the best way to set the RowId?
    //                 history.ItemId = entry.PrimaryKey();
    //                 history.CrudOperation = 'A';
    //                 history.ItemJson = json.ToString(formatting);
    //                 break;

    //             case EntityState.Modified:
    //                 var beforeChange = new JObject();
    //                 var afterChange = new JObject();

    //                 foreach (var prop in properties)
    //                 {
    //                     if (prop.IsModified)
    //                     {
    //                         if (prop.OriginalValue != null)
    //                         {
    //                             if (prop.OriginalValue != prop.CurrentValue)
    //                             {
    //                                 beforeChange[prop.Metadata.Name] = JToken.FromObject(prop.OriginalValue, jsonSerializer);
    //                             }
    //                             else
    //                             {
    //                                 var originalValue = entry.GetDatabaseValues().GetValue<object>(prop.Metadata.Name);
    //                                 beforeChange[prop.Metadata.Name] = originalValue != null
    //                                     ? JToken.FromObject(originalValue, jsonSerializer)
    //                                     : JValue.CreateNull();
    //                             }
    //                         }
    //                         else
    //                         {
    //                             beforeChange[prop.Metadata.Name] = JValue.CreateNull();
    //                         }

    //                         afterChange[prop.Metadata.Name] = prop.CurrentValue != null
    //                         ? JToken.FromObject(prop.CurrentValue, jsonSerializer)
    //                         : JValue.CreateNull();
    //                     }
    //                 }

    //                 history.ItemId = entry.PrimaryKey();
    //                 history.CrudOperation = 'U';
    //                 history.ItemJson = afterChange.ToString(formatting);
    //                 history.OldItemJson = beforeChange.ToString(formatting);
    //                 break;

    //             case EntityState.Deleted:
    //                 foreach (var prop in properties)
    //                 {
    //                     json[prop.Metadata.Name] = prop.OriginalValue != null
    //                         ? JToken.FromObject(prop.OriginalValue, jsonSerializer)
    //                         : JValue.CreateNull();
    //                 }
    //                 history.ItemId = entry.PrimaryKey();
    //                 history.CrudOperation = 'D';
    //                 history.ItemJson = "";
    //                 history.OldItemJson = json.ToString(formatting);
    //                 break;
    //             case EntityState.Detached:
    //             case EntityState.Unchanged:
    //             default:
    //                 throw new NotSupportedException("AutoHistory only support Deleted and Modified entity.");
    //         }

    //         return history;
    //     }

    //     private static string PrimaryKey(this EntityEntry entry)
    //     {
    //         var key = entry.Metadata.FindPrimaryKey();

    //         var values = new List<object>();
    //         foreach (var property in key.Properties)
    //         {
    //             var value = entry.Property(property.Name).CurrentValue;
    //             if (value != null)
    //             {
    //                 values.Add(value);
    //             }
    //         }

    //         return string.Join(",", values);
    //     }
    // }
}