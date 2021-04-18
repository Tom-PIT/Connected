using System;
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
		public SerializationService(ITenant tenant) : base(tenant)
		{
		}
		public T Clone<T>(object instance)
		{
			return (T)Deserialize(Serialize(instance), typeof(T));
		}

		public object Deserialize(byte[] state, Type type)
		{
			var content = Encoding.UTF8.GetString(state);

			try
			{
				var r =  JsonConvert.DeserializeObject(content, type, new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.Auto,
					ContractResolver = SupportInitializeContractResolver.Instance,
					Context = new StreamingContext(StreamingContextStates.Other, Tenant)
				});

				return r;
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
			var data = JsonConvert.SerializeObject(component, Formatting.Indented, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
				Context = new StreamingContext(StreamingContextStates.Other, Tenant)
			});

			return Encoding.UTF8.GetBytes(data);
		}
	}
}