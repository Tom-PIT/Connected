namespace TomPIT.Environment
{
	public interface IClientService
	{
		IClient Select(string token);
		string Insert(string name, ClientStatus status, string type);
		void Update(string token, string name, ClientStatus status, string type);
		void Delete(string token);

		void Notify(string token, string method, object args);
	}
}
