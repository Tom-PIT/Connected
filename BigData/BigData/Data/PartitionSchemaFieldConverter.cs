using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TomPIT.BigData.Data
{
   public class PartitionSchemaFieldConverter : JsonConverter
   {
      static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new PartitionSchemaFieldConcreteConverter() };

      public override bool CanConvert(Type objectType)
      {
         return (objectType == typeof(PartitionSchemaField));
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
         var jo = JObject.Load(reader);

         var type = Type.GetType(jo["type"].Value<string>());

         if (type == typeof(byte)
            || type == typeof(short)
            || type == typeof(int)
            || type == typeof(long)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(decimal))
            return JsonConvert.DeserializeObject<PartitionSchemaNumberField>(jo.ToString(), SpecifiedSubclassConversion);
         else if (type == typeof(string)
            || type == typeof(char)
            || type == typeof(Guid))
            return JsonConvert.DeserializeObject<PartitionSchemaStringField>(jo.ToString(), SpecifiedSubclassConversion);
         else if (type == typeof(DateTime))
            return JsonConvert.DeserializeObject<PartitionSchemaDateField>(jo.ToString(), SpecifiedSubclassConversion);
         else if (type == typeof(bool))
            return JsonConvert.DeserializeObject<PartitionSchemaBoolField>(jo.ToString(), SpecifiedSubclassConversion);

         throw new NotImplementedException();
      }

      public override bool CanWrite
      {
         get { return false; }
      }

      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
         throw new NotImplementedException();
      }
   }
}