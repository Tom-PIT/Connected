using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.SysDb.Environment
{
	public interface IClientHandler
	{
		void Insert(string token, string name, DateTime created, ClientStatus status, string type);
		void Update(IClient client, string name, ClientStatus status, string type);
		void Delete(IClient client);
		IClient Select(string token);
		List<IClient> Query();
	}
}
