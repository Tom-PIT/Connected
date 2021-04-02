using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Reflection
{
	internal class ManifestSymbolReferenceConverter : JsonConverter<Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>>>
	{
		public override Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>> ReadJson(JsonReader reader, Type objectType, Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var data = serializer.Deserialize<JArray>(reader);
			var result = new Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>>();

			foreach(var item in data)
			{
				var key = TypedSerializationDescriptor.Create(item["key"].ToString()).Deserialize<IManifestSymbolReference>();
				var value = new HashSet<IManifestSymbolLocation>();
				var items = Serializer.Deserialize<JArray>(item["items"]);

				foreach(var i in items)
					value.Add(TypedSerializationDescriptor.Create(i.ToString()).Deserialize<IManifestSymbolLocation>());

				result.Add(key, value);
			}

			return result;
		}

		public override void WriteJson(JsonWriter writer, Dictionary<IManifestSymbolReference, HashSet<IManifestSymbolLocation>> value, JsonSerializer serializer)
		{
			var array = new JArray();

			foreach (var item in value)
			{
				var jo = new JObject
				{
					{"key", Serializer.Serialize(new TypedSerializationDescriptor(item.Key))}
				};

				var items = new JArray();

				foreach(var hashItem in item.Value)
				{
					if (hashItem == null)
						continue;

					items.Add(Serializer.Serialize(new TypedSerializationDescriptor(hashItem)));
				}

				jo.Add("items", items);

				array.Add(jo);
			}

			serializer.Serialize(writer, array);
		}
	}
}
