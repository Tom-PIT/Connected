using TomPIT.Environment;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareClient
	{
		void Notify(string token, string method, object arguments);
		IClient Select(string token);
		string Insert(string name, string type);
		void Update(string token, string name, ClientStatus status, string type);
		void Delete(string token);
	}
}
