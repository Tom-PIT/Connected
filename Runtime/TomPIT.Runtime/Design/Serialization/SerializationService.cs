using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Design.Serialization
{
	internal class SerializationService : TenantObject, ISerializationService
	{
		private Lazy<ConcurrentDictionary<string, string>> _replacements = new Lazy<ConcurrentDictionary<string, string>>();
		public SerializationService(ITenant tenant) : base(tenant)
		{
		}
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
					Context = new StreamingContext(StreamingContextStates.Other, Tenant)
				});
			}
			catch (Exception ex)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDeserialize, type.TypeName()), ex)
				{
					EventId = MiddlewareEvents.Deserialize,
				};
			}
		}

		public byte[] Serialize(object component)
		{
			var data = JsonConvert.SerializeObject(component, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
				Context = new StreamingContext(StreamingContextStates.Other, Tenant)
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