using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using TomPIT.Net;

namespace TomPIT.Design.Serialization
{
	internal class SerializationService : ISerializationService
	{
		public SerializationService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public T Clone<T>(object instance)
		{
			return (T)Deserialize(Serialize(instance), typeof(T));
		}

		public object Deserialize(byte[] state, Type type)
		{
			return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(state), type, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto,
				ContractResolver = SupportInitializeContractResolver.Instance,
				Context = new StreamingContext(StreamingContextStates.Other, Server)
			});
		}

		public byte[] Serialize(object component)
		{
			var data = JsonConvert.SerializeObject(component, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
				Context = new StreamingContext(StreamingContextStates.Other, Server)
			});

			return Encoding.UTF8.GetBytes(data);
		}
	}
}