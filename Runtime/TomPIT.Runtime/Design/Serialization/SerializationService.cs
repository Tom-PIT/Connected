using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text;
using TomPIT.Connectivity;
using TomPIT.Services;

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

			try
			{
				return JsonConvert.DeserializeObject(content, type, new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.Auto,
					ContractResolver = SupportInitializeContractResolver.Instance,
					Context = new StreamingContext(StreamingContextStates.Other, Connection)
				});
			}
			catch (Exception ex)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDeserialize, type.TypeName()), ex)
				{
					EventId = ExecutionEvents.Deserialize,
				};
			}
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

		private ConcurrentDictionary<string, string> Replacements { get { return _replacements.Value; } }

		public void RegisterReplacement(string obsolete, string replacement)
		{
			if (!Replacements.ContainsKey(obsolete))
				Replacements.TryAdd(obsolete, replacement);
		}
	}
}