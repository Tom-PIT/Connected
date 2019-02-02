using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class MicroServiceElement : Element, IMicroServiceScope
	{
		private List<IFolder> _folders = null;
		private List<IComponent> _components = null;

		public MicroServiceElement(IEnvironment environment, Guid microService) : base(environment, null)
		{
			MicroService = GetService<IMicroServiceService>().Select(microService);

			Id = microService.ToString();
			Title = MicroService.Name;
			Glyph = "fal fa-share-alt";

			Template = GetService<IMicroServiceTemplateService>().Select(MicroService.Template);

			((Behavior)Behavior).Static = true;
		}

		public IMicroService MicroService { get; }
		private IMicroServiceTemplate Template { get; }

		public override object Component => MicroService;
		public override int ChildrenCount => Components.Count + Folders.Count;
		public override bool HasChildren => ChildrenCount > 0;

		public override void LoadChildren()
		{
			var refs = Template.References(Environment, MicroService.Token);

			Items.Add(new ReferencesElement(this, refs));

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
			var folder = Folders.FirstOrDefault(f => f.Token == id.AsGuid());

			if (folder != null)
			{
				Items.Add(new FolderElement(this, folder));
				return;
			}

			var component = Components.FirstOrDefault(f => f.Token == id.AsGuid());

			if (component != null)
			{
				if (string.Compare(component.Category, "Reference", true) == 0)
					Items.Add(new ReferencesElement(this, component));
				else
					Items.Add(component.GetDomElement(this));
			}
		}

		private List<IFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = GetService<IComponentService>().QueryFolders(MicroService.Token, Guid.Empty);

				return _folders;
			}
		}

		private List<IComponent> Components
		{
			get
			{
				if (_components == null)
					_components = GetService<IComponentService>().QueryComponents(MicroService.Token, Guid.Empty);

				return _components;
			}
		}
	}
}
