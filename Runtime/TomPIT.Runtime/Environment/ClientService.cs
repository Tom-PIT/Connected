using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
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
			Tenant.Post(CreateUrl("Delete"), new
			{
				token
			});

			Remove(token);
		}

		public string Insert(string name, ClientStatus status, string type)
		{
			return Tenant.Post<string>(CreateUrl("Insert"), new
			{
				name,
				status,
				type
			});
		}

		public void Notify(string token, string method, object arguments)
		{
			var cdn = Tenant.GetService<IInstanceEndpointService>().Select(InstanceType.Cdn);

			if (cdn == null)
				throw new NotFoundException($"{SR.ErrInstanceEndpointNotFound} ({InstanceType.Cdn})");

			var url = $"{cdn.Url}/sys/clients/notify";
			var provider = Tenant.GetService<IAuthorizationService>() as IAuthenticationTokenProvider;
			var authToken = string.Empty;

			if (provider != null)
				authToken = provider.RequestToken(InstanceType.Cdn);

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
				return Tenant.Post<Client>(CreateUrl("Select"), new
				{
					token
				});
			});
		}

		public void Update(string token, string name, ClientStatus status, string type)
		{
			Tenant.Post(CreateUrl("Update"), new
			{
				token,
				name,
				status,
				type
			});
		}

		private ServerUrl CreateUrl(string method)
		{
			return Tenant.CreateUrl("Client", method);
		}
	}
}
