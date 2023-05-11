using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Environment;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Environment
{
	public class ClientsModel : SynchronizedRepository<IClient, string>
	{
		public ClientsModel(IMemoryCache container) : base(container, "client")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Environment.Clients.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Environment.Clients.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IClient Select(string token)
		{
			return Get(token);
		}

		public string Insert(string name, ClientStatus status, string type)
		{
			var token = Guid.NewGuid().ToString().ToUpper().Replace("-", string.Empty);

			Shell.GetService<IDatabaseService>().Proxy.Environment.Clients.Insert(token, name, DateTime.UtcNow, status, type);

			Refresh(token);
			CachingNotifications.ClientChanged(token);

			return token;
		}

		public void Update(string token, string name, ClientStatus status, string type)
		{
			var client = Select(token);

			if (client == null)
				throw new SysException(SR.ErrClientNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Environment.Clients.Update(client, name, status, type);

			Refresh(client.Token);
			CachingNotifications.ClientChanged(client.Token);
		}

		public ImmutableList<IClient> Query()
		{
			return All();
		}

		public void Delete(string token)
		{
			var client = Select(token);

			if (client == null)
				throw new SysException(SR.ErrClientNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Environment.Clients.Delete(client);

			Remove(token);
			CachingNotifications.ClientChanged(token);
		}
	}
}