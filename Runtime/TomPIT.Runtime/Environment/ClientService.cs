using TomPIT.Caching;
using TomPIT.Cdn;
using TomPIT.Connectivity;

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
			Tenant.GetService<ICdnClientNotificationProxy>().Notify(token, method, arguments);
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
