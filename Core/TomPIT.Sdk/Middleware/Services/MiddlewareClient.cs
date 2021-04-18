using TomPIT.Environment;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareClient : MiddlewareObject, IMiddlewareClient
	{
		public void Delete(string token)
		{
			Context.Tenant.GetService<IClientService>().Delete(token);
		}

		public string Insert(string name, string type)
		{
			return Context.Tenant.GetService<IClientService>().Insert(name, ClientStatus.Enabled, type);
		}

		public void Notify(string token, string method, object arguments)
		{
			Context.Tenant.GetService<IClientService>().Notify(token, method, arguments);
		}

		public IClient Select(string token)
		{
			return Context.Tenant.GetService<IClientService>().Select(token);
		}

		public void Update(string token, string name, ClientStatus status, string type)
		{
			Context.Tenant.GetService<IClientService>().Update(token, name, status, type);
		}
	}
}
