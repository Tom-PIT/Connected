using System;
using TomPIT.ComponentModel;
using TomPIT.Deployment;

namespace TomPIT.Design
{
	public interface IComponentDevelopmentService
	{
		Guid Insert(Guid microService, Guid folder, string category, string name, string type);
		void Restore(Guid microService, IPackageComponent component, IPackageBlob configuration, IPackageBlob runtimeConfiguration);
		void Update(Guid component, string name, Guid folder);
		void Update(IConfiguration configuration);
		void Update(IText text, string content);
		void Delete(Guid component, bool permanent);

		string CreateName(Guid microService, string category, string prefix);

		Guid InsertFolder(Guid microService, Guid token, string name, Guid parent);
		void UpdateFolder(Guid microService, Guid folder, string name, Guid parent);
		void DeleteFolder(Guid microService, Guid folder, bool permanent);
	}
}
