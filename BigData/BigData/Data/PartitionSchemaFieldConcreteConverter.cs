using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TomPIT.BigData.Data
{
   internal class PartitionSchemaFieldConcreteConverter : DefaultContractResolver
   {
      protected override JsonConverter ResolveContractConverter(Type objectType)
      {
         if (typeof(PartitionSchemaField).IsAssignableFrom(objectType) && !objectType.IsAbstract)
            return null;

         return base.ResolveContractConverter(objectType);
      }
   }
}