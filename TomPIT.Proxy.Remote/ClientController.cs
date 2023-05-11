using TomPIT.Environment;

namespace TomPIT.Proxy.Remote
{
	internal class ClientController : IClientController
	{
		private const string Controller = "Client";
		public void Delete(string token)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
			{
				token
			});
		}

		public string Insert(string name, ClientStatus status, string type)
		{
			return Connection.Post<string>(Connection.CreateUrl(Controller, "Insert"), new
			{
				name,
				status,
				type
			});
		}

		public IClient Select(string token)
		{
			return Connection.Post<Client>(Connection.CreateUrl(Controller, "Select"), new
			{
				token
			});
		}

		public void Update(string token, string name, ClientStatus status, string type)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Update"), new
			{
				token,
				name,
				status,
				type
			});
		}
	}
}
