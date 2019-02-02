using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	public delegate void ComponentChangedHandler(ISysConnection sender, ComponentEventArgs e);
	public delegate void ConfigurationChangedHandler(ISysConnection sender, ConfigurationEventArgs e);
	public delegate void FolderChangedHandler(object sender, FolderEventArgs e);

	public interface IComponentService
	{
		event ComponentChangedHandler ComponentChanged;
		event ConfigurationChangedHandler ConfigurationChanged;
		event ConfigurationChangedHandler ConfigurationAdded;
		event ConfigurationChangedHandler ConfigurationRemoved;
		event FolderChangedHandler FolderChanged;

		IFolder SelectFolder(Guid folder);
		List<IFolder> QueryFolders(Guid microService, Guid parent);

		List<IConfiguration> QueryConfigurations(List<IComponent> components);
		List<IConfiguration> QueryConfigurations(List<string> resourceGroups, string categories);
		IComponent SelectComponent(Guid microService, string category, string name);
		IComponent SelectComponent(string category, string name);
		IComponent SelectComponent(Guid token);
		IConfiguration SelectConfiguration(Guid microService, string category, string name);
		IConfiguration SelectConfiguration(Guid component);

		string SelectText(Guid microService, IText text);

		List<IComponent> QueryComponents(Guid microService, string category);
		List<IComponent> QueryComponents(Guid microService, Guid folder);
		List<IComponent> QueryComponents(Guid microService);


		string CreateName(Guid microService, string category, string prefix);
	}
}
