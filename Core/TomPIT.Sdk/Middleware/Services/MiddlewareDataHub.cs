using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Serialization;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareDataHub : MiddlewareObject, IMiddlewareDataHub
	{
		private string _dataHubServer = null;

		public string Server
		{
			get
			{
				if (_dataHubServer == null)
				{
					_dataHubServer = Context.Services.Routing.GetServer(InstanceType.Cdn, InstanceVerbs.All);

					if (_dataHubServer == null)
						throw new RuntimeException(SR.ErrNoCdnServer);

					_dataHubServer = $"{_dataHubServer}/dataHub";
				}

				return _dataHubServer;
			}
		}

		public void Notify([CIP(CIP.DataHubEndpointProvider)] string dataHubEndpoint, object e)
		{
			var descriptor = ComponentDescriptor.DataHub(Context, dataHubEndpoint);

			descriptor.Validate();

			var endpoint = Context.Services.Routing.GetServer(InstanceType.Cdn, InstanceVerbs.Post);

			if (endpoint == null)
				throw new RuntimeException(SR.ErrNoCdnServer);

			var url = $"{endpoint}/data";
			var args = new JObject
			{
				{"endpoint",  $"{descriptor.MicroService.Name}/{descriptor.Component.Name}/{descriptor.Element}"}
			};

			if (e != null)
				args.Add("arguments", Serializer.Deserialize<JObject>(Serializer.Serialize(e)));

			Context.Tenant.Post(url, args);
		}

	}
}
