﻿using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Deployment;

namespace TomPIT.Design
{
	public interface IComponentModel
	{
		event EventHandler<FileArgs> FileRestored;
		event EventHandler<ComponentArgs> ComponentRestored;
		event EventHandler<ComponentArgs> ConfigurationRestored;

		event EventHandler<FileArgs> FileDeleted;

		List<IComponent> Query(Guid microService);
		List<IComponent> Query(Guid[] microServices);
		Guid Insert(Guid microService, Guid folder, string category, string name, string type);
		void Restore(Guid microService, IPullRequestComponent component);
		[Obsolete]
		void Restore(Guid microService, IPackageComponent component, IPackageBlob configuration);
		Guid Clone(Guid component, Guid microService, Guid folder);
		void Update(Guid component, string name, Guid folder);
		void Update(IConfiguration configuration);
		void Update(IConfiguration configuration, ComponentUpdateArgs e);
		void Update(IText text, string content);
		void Delete(Guid component);
		void Delete(Guid component, bool permanent);

		string CreateName(Guid microService, string category, string prefix);

		Guid InsertFolder(Guid microService, string name, Guid parent);
		void UpdateFolder(Guid microService, Guid folder, string name, Guid parent);
		void DeleteFolder(Guid microService, Guid folder, bool deleteComponents);
		void RestoreFolder(Guid microService, Guid token, string name, Guid parent);

		IComponentImage CreateComponentImage(Guid component);
		IComponentImage SelectComponentImage(Guid blob);
		[Obsolete]
		void RestoreComponent(IComponentImage image);
		[Obsolete]
		void RestoreComponent(Guid blob);
	}
}
