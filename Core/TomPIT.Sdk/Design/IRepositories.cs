using System.Collections.Generic;
using TomPIT.Development;

namespace TomPIT.Design
{
	public interface IRepositories
	{
		List<IRepositoriesEndpoint> Query();
		void Insert(string name, string url, string userName, string password);
		void Update(string existingName, string name, string url, string userName, string password);
		void Delete(string name);
		IRepositoriesEndpoint Select(string name);
	}
}
