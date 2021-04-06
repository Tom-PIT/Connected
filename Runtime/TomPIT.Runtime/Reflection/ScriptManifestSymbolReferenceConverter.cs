using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Reflection
{
	internal class ScriptManifestSymbolReferenceConverter : JsonConverter<Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>>>
	{
		public override Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>> ReadJson(JsonReader reader, Type objectType, Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var data = serializer.Deserialize<JArray>(reader);
			var result = new Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>>();

			foreach(var item in data)
			{
				var key = TypedSerializationDescriptor.Create(item["key"].ToString()).Deserialize<IScriptManifestSymbolReference>();
				var value = new HashSet<IScriptManifestSymbolLocation>();
				var items = Serializer.Deserialize<JArray>(item["items"]);

				foreach(var i in items)
					value.Add(TypedSerializationDescriptor.Create(i.ToString()).Deserialize<IScriptManifestSymbolLocation>());

				result.Add(key, value);
			}

			return result;
		}

		public override void WriteJson(JsonWriter writer, Dictionary<IScriptManifestSymbolReference, HashSet<IScriptManifestSymbolLocation>> value, JsonSerializer serializer)
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
