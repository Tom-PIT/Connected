﻿using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Security;

namespace TomPIT.Dom
{
	public class FolderElement : TransactionElement, IFolderScope, IPermissionElement
	{
		private List<IFolder> _folders = null;
		private List<IComponent> _components = null;
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

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

		public List<string> Claims
		{
			get
			{
				if (_claims == null)
					_claims = new List<string>
					{
						TomPIT.Claims.FolderAccess
					};

				return _claims;
			}
		}

		public IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new FolderPermissionDescriptor();

				return _descriptor;
			}
		}

		public string PrimaryKey => Folder.Token.ToString();

		public virtual bool SupportsInherit => Folder.Parent != Guid.Empty;
		public virtual string PermissionComponent => null;
		public virtual Guid ResourceGroup => DomQuery.Closest<IMicroServiceScope>(this).MicroService.ResourceGroup;

		public override void LoadChildren()
		{
			foreach (var i in Folders.OrderBy(f => f.Name))
				Items.Add(new FolderElement(this, i));

			foreach (var i in Components.OrderBy(f => f.Name))
				Items.Add(i.GetDomElement(this));
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
				Items.Add(component.GetDomElement(this));
		}

		private List<IFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = GetService<IComponentService>().QueryFolders(DomQuery.Closest<IMicroServiceScope>(this).MicroService.Token, Folder.Token);

				return _folders;
			}
		}

		private List<IComponent> Components
		{
			get
			{
				if (_components == null)
					_components = GetService<IComponentService>().QueryComponents(DomQuery.Closest<IMicroServiceScope>(this).MicroService.Token, Folder.Token);

				return _components;
			}
		}

		public override bool Commit(object component, string property, string attribute)
		{
			GetService<IComponentDevelopmentService>().UpdateFolder(Folder.MicroService, Folder.Token, Folder.Name, Folder.Parent);

			return true;
		}
	}
}