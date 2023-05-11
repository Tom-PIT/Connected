using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Security;

namespace TomPIT.Environment
{
	internal class ClientService : ClientRepository<IClient, string>, IClientService, IClientNotification
	{
		public ClientService(ITenant connection) : base(connection, "client")
		{

		}

		public void Delete(string token)
		{
			Instance.SysProxy.Clients.Delete(token);

			Remove(token);
		}

		public string Insert(string name, ClientStatus status, string type)
		{
			return Instance.SysProxy.Clients.Insert(name, status, type);
		}

		public void Notify(string token, string method, object arguments)
		{
			var cdn = Tenant.GetService<IInstanceEndpointService>().Select(InstanceFeatures.Cdn);

			if (cdn == null)
				throw new NotFoundException($"{SR.ErrInstanceEndpointNotFound} ({InstanceFeatures.Cdn})");

			var url = $"{cdn.Url}/sys/clients/notify";
			var provider = Tenant.GetService<IAuthorizationService>() as IAuthenticationTokenProvider;
			var authToken = string.Empty;

			if (provider != null)
				authToken = provider.RequestToken(InstanceFeatures.Cdn);

			Tenant.Post(url, new
			{
				token,
				method,
				arguments
			}, new HttpRequestArgs().WithBearerCredentials(authToken));
		}

		public void NotifyChanged(object sender, ClientEventArgs e)
		{
			Remove(e.Token);
		}

		public IClient Select(string token)
		{
			return Get(token, (f) =>
			{
				return Instance.SysProxy.Clients.Select(token);
			});
		}

		public void Update(string token, string name, ClientStatus status, string type)
		{
			Instance.SysProxy.Clients.Update(token, name, status, type);
		}
	}
}
