using System;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Ide.Verbs;

namespace TomPIT.Dom
{
	public class FolderElement : TransactionElement, IFolderScope
	{
		private ImmutableList<IFolder> _folders = null;
		private ImmutableList<IComponent> _components = null;

		public FolderElement(IDomElement parent, IFolder folder) : base(parent)
		{
			Folder = folder;

			Id = Folder.Token.ToString();
			Title = folder.Name;
			Glyph = "fal fa-folder dom-element-warning";

			Verbs.Add(new Verb
			{
				Action = VerbAction.Ide,
				Confirm = string.Format("Are you sure you want to delete folder '{0}'?", Title),
				Id = "deleteFolder",
				Name = "Delete folder"
			});

			((Behavior)Behavior).Container = true;
			((Behavior)Behavior).Static = false;
		}

		public IFolder Folder { get; }
		public override object Component => Folder;

		public override int ChildrenCount => Components.Count + Folders.Count;
		public override bool HasChildren => ChildrenCount > 0;

		public override void LoadChildren()
		{
			foreach (var i in Folders.OrderBy(f => f.Name))
				Items.Add(new FolderElement(this, i));

			foreach (var i in Components.OrderBy(f => f.Name))
				Items.Add(i.GetDomElement(this));
		}

		public override void LoadChildren(string id)
		{
			var folder = Folders.FirstOrDefault(f => f.Token == new Guid(id));

			if (folder != null)
			{
				Items.Add(new FolderElement(this, folder));
				return;
			}

			var component = Components.FirstOrDefault(f => f.Token == new Guid(id));

			if (component != null)
				Items.Add(component.GetDomElement(this));
		}

		private ImmutableList<IFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = Environment.Context.Tenant.GetService<IComponentService>().QueryFolders(DomQuery.Closest<IMicroServiceScope>(this).MicroService.Token, Folder.Token);

				return _folders;
			}
		}

		private ImmutableList<IComponent> Components
		{
			get
			{
				if (_components == null)
					_components = Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(DomQuery.Closest<IMicroServiceScope>(this).MicroService.Token, Folder.Token);

				return _components;
			}
		}

		public override bool Commit(object component, string property, string attribute)
		{
			Environment.Context.Tenant.GetService<IDesignService>().Components.UpdateFolder(Folder.MicroService, Folder.Token, Folder.Name, Folder.Parent);

			return true;
		}
	}
}
