using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	public delegate void ComponentChangedHandler(ITenant sender, ComponentEventArgs e);
	public delegate void ConfigurationChangedHandler(ITenant sender, ConfigurationEventArgs e);
	public delegate void FolderChangedHandler(object sender, FolderEventArgs e);

	public interface IComponentService
	{
		event ComponentChangedHandler ComponentChanged;
		event ComponentChangedHandler ComponentAdded;
		event ComponentChangedHandler ComponentRemoved;
		event ConfigurationChangedHandler ConfigurationChanged;
		event ConfigurationChangedHandler ConfigurationAdded;
		event ConfigurationChangedHandler ConfigurationRemoved;
		event FolderChangedHandler FolderChanged;

		IFolder SelectFolder(Guid folder);
		List<IFolder> QueryFolders(Guid microService, Guid parent);
		List<IFolder> QueryFolders(Guid microService);

		List<IConfiguration> QueryConfigurations(List<IComponent> components);
		List<IConfiguration> QueryConfigurations(List<string> resourceGroups, string categories);
		List<IConfiguration> QueryConfigurations(Guid microService, string categories);
		IComponent SelectComponent(Guid microService, string category, string name);
		IComponent SelectComponentByNameSpace(Guid microService, string nameSpace, string name);
		//IComponent SelectComponent(string category, string name);
		IComponent SelectComponent(Guid token);
		IConfiguration SelectConfiguration(Guid microService, string category, string name);
		IConfiguration SelectConfiguration(Guid component);

		string SelectText(Guid microService, IText text);

		List<IComponent> QueryComponents(Guid microService, string category);
		List<IComponent> QueryComponents(Guid microService, Guid folder);
		List<IComponent> QueryComponents(Guid microService);
		List<IComponent> QueryComponents(List<string> resourceGroups, string categories);


		string CreateName(Guid microService, string category, string prefix);
	}
}
