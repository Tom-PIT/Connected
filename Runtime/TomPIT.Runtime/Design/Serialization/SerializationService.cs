using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using TomPIT.Connectivity;

namespace TomPIT.Design.Serialization
{
	internal class SerializationService : ISerializationService
	{
		private Lazy<ConcurrentDictionary<string, string>> _replacements = new Lazy<ConcurrentDictionary<string, string>>();
		public SerializationService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public T Clone<T>(object instance)
		{
			return (T)Deserialize(Serialize(instance), typeof(T));
		}

		public object Deserialize(byte[] state, Type type)
		{
			var content = ResolveTypes(Encoding.UTF8.GetString(state));

			return JsonConvert.DeserializeObject(content, type, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto,
				ContractResolver = SupportInitializeContractResolver.Instance,
				Context = new StreamingContext(StreamingContextStates.Other, Connection)
			});
		}

		public byte[] Serialize(object component)
		{
			var data = JsonConvert.SerializeObject(component, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
				Context = new StreamingContext(StreamingContextStates.Other, Connection)
			});

			return Encoding.UTF8.GetBytes(data);
		}

		private string ResolveTypes(string content)
		{
			if (Replacements.Count == 0)
				return content;


			return content;
		}

		private ConcurrentDictionary<string,string> Replacements { get { return _replacements.Value; } }

		public void RegisterReplacement(string obsolete, string replacement)
		{
			if (!Replacements.ContainsKey(obsolete))
				Replacements.TryAdd(obsolete, replacement);
		}
	}
}