using TomPIT.Environment;

namespace TomPIT.Proxy
{
	public interface IClientController
	{
		IClient Select(string token);
		string Insert(string name, ClientStatus status, string type);
		void Update(string token, string name, ClientStatus status, string type);
		void Delete(string token);
	}
}
