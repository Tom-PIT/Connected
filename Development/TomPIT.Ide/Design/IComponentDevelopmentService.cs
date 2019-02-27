using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Deployment;
using TomPIT.Ide.Design;

namespace TomPIT.Design
{
	public interface IComponentDevelopmentService
	{
		Guid Insert(Guid microService, Guid folder, string category, string name, string type);
		void Restore(Guid microService, IPackageComponent component, IPackageBlob configuration, IPackageBlob runtimeConfiguration);
		void Update(Guid component, string name, Guid folder);
		void Update(IConfiguration configuration);
		void Update(IText text, string content);
		void Delete(Guid component);

		string CreateName(Guid microService, string category, string prefix);

		Guid InsertFolder(Guid microService, string name, Guid parent);
		void UpdateFolder(Guid microService, Guid folder, string name, Guid parent);
		void DeleteFolder(Guid microService, Guid folder);
		void RestoreFolder(Guid microService, Guid token, string name, Guid parent);

		void SaveRuntimeState(Guid microService);
		void DropRuntimeState(Guid microService);
		Dictionary<Guid, Guid> SelectRuntimeState(Guid microService);

		IComponentImage CreateComponentImage(Guid component);
		void RestoreComponent(IComponentImage image);
		void RestoreComponent(Guid blob);
		void Import(Guid microService, Guid blob);
	}
}
