using System;

namespace TomPIT.Proxy.Development
{
	public interface IFolderDevelopmentController
	{
		Guid Insert(Guid microService, string name, Guid parent);
		void Update(Guid microService, Guid folder, string name, Guid parent);
		void Delete(Guid microService, Guid folder);
		void Restore(Guid microService, Guid token, string name, Guid parent);
	}
}
