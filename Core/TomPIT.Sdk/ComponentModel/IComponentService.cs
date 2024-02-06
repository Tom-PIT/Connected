using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
		event ComponentChangedHandler Deleting;
		event ConfigurationChangedHandler ConfigurationChanged;
		event ConfigurationChangedHandler ConfigurationAdded;
		event ConfigurationChangedHandler ConfigurationRemoved;
		event FolderChangedHandler FolderChanged;

		IFolder SelectFolder(Guid folder);
		ImmutableList<IFolder> QueryFolders(Guid microService, Guid parent);
		ImmutableList<IFolder> QueryFolders(Guid microService);

		ImmutableList<IConfiguration> QueryConfigurations(ImmutableList<IComponent> components);

		ImmutableList<IConfiguration> QueryConfigurations(List<string> resourceGroups, string categories);
		ImmutableList<IConfiguration> QueryConfigurations(List<Guid> microServices, string categories);
		ImmutableList<IConfiguration> QueryConfigurations(params string[] categories);
		ImmutableList<IConfiguration> QueryConfigurations(string categories);
		ImmutableList<IConfiguration> QueryConfigurations(Guid microService, string categories);
		IComponent SelectComponent(Guid microService, string category, string name);
		//IComponent SelectComponent(Guid microService, Guid folder, string category, string name);
		IComponent SelectComponentByNameSpace(Guid microService, string nameSpace, string name);
		IComponent SelectComponent(Guid token);
		IConfiguration SelectConfiguration(Guid microService, string category, string name);
		IConfiguration SelectConfiguration(Guid component);

		string SelectText(Guid microService, IText text);

		ImmutableList<IComponent> QueryComponents(Guid microService, string category);
		ImmutableList<IComponent> QueryComponents(Guid microService, Guid folder);
		ImmutableList<IComponent> QueryComponents(Guid microService);
		ImmutableList<IComponent> QueryComponents(params string[] categories);
	}
}
