using System;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Ide;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Dom;

namespace TomPIT.Dom
{
	public class MicroServiceElement : DomElement, IMicroServiceScope
	{
		private ImmutableList<IFolder> _folders = null;
		private ImmutableList<IComponent> _components = null;

		public MicroServiceElement(IEnvironment environment, Guid microService) : base(environment, null)
		{
			MicroService = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			Id = microService.ToString();
			Title = MicroService.Name;
			Glyph = "fal fa-share-alt";

			Template = Environment.Context.Tenant.GetService<IMicroServiceTemplateService>().Select(MicroService.Template);

			((Behavior)Behavior).Static = true;
			((Behavior)Behavior).Container = true;
		}

		public IMicroService MicroService { get; }
		private IMicroServiceTemplate Template { get; }

		public override object Component => MicroService;
		public override int ChildrenCount => 0;
		public override bool HasChildren => Components.Count + Folders.Count > 0;

		public override void LoadChildren()
		{
			var refs = Template.References(Environment, MicroService.Token);

			Items.Add(new ReferencesElement(this, refs));
			Items.Add(new VersionControlElement(this));

			foreach (var i in Folders.OrderBy(f => f.Name))
				Items.Add(new FolderElement(this, i));

			foreach (var i in Components.OrderBy(f => f.Name))
			{
				if (string.Compare(i.Category, "Reference", true) == 0)
					continue;

				Items.Add(i.GetDomElement(this));
			}
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, VersionControlElement.ElementId, true) == 0)
			{
				Items.Add(new VersionControlElement(this));
				return;
			}

			var folder = Folders.FirstOrDefault(f => f.Token == new Guid(id));

			if (folder != null)
			{
				Items.Add(new FolderElement(this, folder));
				return;
			}

			var component = Components.FirstOrDefault(f => f.Token == new Guid(id));

			if (component != null)
			{
				if (string.Compare(component.Category, "Reference", true) == 0)
					Items.Add(new ReferencesElement(this, component));
				else
					Items.Add(component.GetDomElement(this));
			}
		}

		private ImmutableList<IFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = Environment.Context.Tenant.GetService<IComponentService>().QueryFolders(MicroService.Token, Guid.Empty);

				return _folders;
			}
		}

		private ImmutableList<IComponent> Components
		{
			get
			{
				if (_components == null)
					_components = Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(MicroService.Token, Guid.Empty);

				return _components;
			}
		}
	}
}
