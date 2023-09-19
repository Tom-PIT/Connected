using TomPIT.Environment;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class ClientController : IClientController
	{
		public void Delete(string token)
		{
			DataModel.Clients.Delete(token);
		}

		public string Insert(string name, ClientStatus status, string type)
		{
			return DataModel.Clients.Insert(name, status, type);
		}

		public IClient Select(string token)
		{
			return DataModel.Clients.Select(token);
		}

		public void Update(string token, string name, ClientStatus status, string type)
		{
			DataModel.Clients.Update(token, name, status, type);
		}
	}
}
