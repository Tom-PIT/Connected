using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Models;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Development.Models
{
	public class IdeModel : IdeModelBase
	{
		private IMicroServiceTemplate _template = null;

		private IMicroServiceTemplate Template
		{
			get
			{
				if (_template == null)
					_template = Tenant.GetService<IMicroServiceTemplateService>().Select(MicroService.Template);

				return _template;
			}
		}

		protected override IDom CreateDom()
		{
			var path = string.IsNullOrWhiteSpace(Path)
				? RequestBody?.Optional("path", string.Empty)
				: Path;


			return new Ide.Dom(this, path);
		}

		public override string Id => MicroService.Token.ToString();
		public override string IdeUrl
		{
			get
			{
				return MiddlewareDescriptor.Current.RouteUrl(this, "ide", new { microService = MicroService.Url });
			}
		}

		protected override void OnDatabinding()
		{
			Title = MicroService?.Name;
		}

		protected override IDesignerActionResult CreateItem(JObject data)
		{
			var item = data.Required<string>("item");
			var name = data.Required<string>("name");
			var descriptor = Selection.AddItems.FirstOrDefault(f => string.Compare(f.Id, item, true) == 0);

			var instance = descriptor.Type.CreateInstance();

			if (instance == null)
				throw IdeException.CannotCreateInstance(this, IdeEvents.IdeAction, descriptor.Type);

			IdeExtensions.ProcessComponentCreating(this, instance);

			if (instance is IFolder folder)
				return CreateFolder(folder, name);
			else if (instance is IConfiguration config)
				return CreateComponent(config, name, descriptor);

			return Result.EmptyResult(this);
		}

		private IDesignerActionResult CreateComponent(IConfiguration config, string name, IItemDescriptor descriptor)
		{
			var selection = Selection.Element;

			if (selection == null)
				throw new IdeException(SR.ErrExpectedSelectedElement);

			var parent = Guid.Empty;
			var fs = FindFolder(selection);

			if (fs != null)
				parent = fs.Token;

			var ms = DomQuery.Closest<IMicroServiceScope>(selection);
			var category = descriptor.Value == null ? descriptor.Id.ToString() : descriptor.Value.ToString();
			var id = Tenant.GetService<IDesignService>().Components.Insert(ms.MicroService.Token, fs == null ? Guid.Empty : fs.Token, category, name, descriptor.Type.TypeName());

			config.Component = id;

			IdeExtensions.ProcessComponentCreated(this, config);
			Tenant.GetService<IDesignService>().Components.Update(config);

			var r = Result.SectionResult(this, EnvironmentSection.Explorer);
			var target = fs == null ? selection.Root() : selection.Closest<IFolderScope>() as IDomElement;

			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(target), id);

			((Ide.Dom)Dom).SetPath(DomQuery.Path(target));
			Selection.Reset();

			return r;
		}

		private IDesignerActionResult CreateFolder(IFolder folder, string name)
		{
			var selection = Selection.Element;

			if (selection == null)
				throw new IdeException(SR.ErrExpectedSelectedElement);

			var parent = Guid.Empty;
			var fs = FindFolder(selection);

			if (fs != null)
				parent = fs.Token;

			var ms = DomQuery.Closest<IMicroServiceScope>(selection);
			var id = Tenant.GetService<IDesignService>().Components.InsertFolder(ms.MicroService.Token, name, parent);
			var r = Result.SectionResult(this, EnvironmentSection.Explorer);

			var target = fs == null ? selection.Root() : selection.Closest<IFolderScope>() as IDomElement;

			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(target), id);

			((Ide.Dom)Dom).SetPath(DomQuery.Path(target));
			Selection.Reset();

			return r;
		}

		private IFolder FindFolder(IDomElement element)
		{
			var scope = element.Closest<IFolderScope>();

			if (scope == null)
				return null;

			return scope.Folder;
		}

		protected override IDesignerActionResult DeleteFolder(JObject data)
		{
			if (!(Selection.Element.Component is IFolder folder))
				return Result.EmptyResult(this);

			var path = DomQuery.Path(Selection.Element);
			Tenant.GetService<IDesignService>().Components.DeleteFolder(DomQuery.Closest<IMicroServiceScope>(Selection.Element).MicroService.Token, folder.Token);

			var r = Result.SectionResult(this, EnvironmentSection.Explorer);

			r.ExplorerPath = path.Substring(0, path.LastIndexOf('/'));

			((Ide.Dom)Dom).SetPath(r.ExplorerPath);

			Selection.Reset();
			return r;
		}

		protected override IDesignerActionResult DeleteComponent(JObject data)
		{
			Guid component = Guid.Empty;
			if (Selection.Element.Component is IConfiguration config)
				component = config.Component;
			else if (Selection.Element.Component is IComponent cmp)
				component = cmp.Token;

			if (component == Guid.Empty)
				return Result.EmptyResult(this);

			var path = DomQuery.Path(Selection.Element);
			Tenant.GetService<IDesignService>().Components.Delete(component);

			var r = Result.SectionResult(this, EnvironmentSection.Explorer);

			r.ExplorerPath = path.Substring(0, path.LastIndexOf('/'));

			((Ide.Dom)Dom).SetPath(r.ExplorerPath);

			Selection.Reset();
			return r;
		}

		protected override IDesignerActionResult Move(JObject data)
		{
			var id = data.Required<Guid>("id");
			var folder = data.Optional("folder", Guid.Empty);

			if (folder != Guid.Empty)
			{
				var f = Tenant.GetService<IComponentService>().SelectFolder(folder);
				/*
				 * it's a microservice not a folder
				 */
				if (f == null)
					folder = Guid.Empty;
			}

			var target = Tenant.GetService<IComponentService>().SelectComponent(id);

			if (target != null)
				MoveComponent(target, folder);
			else
			{
				var targetFolder = Tenant.GetService<IComponentService>().SelectFolder(id);

				if (targetFolder != null)
					MoveFolder(targetFolder, folder);
			}

			var selection = Selection.Path;

			if (selection.EndsWith(id.ToString()))
			{
				((Ide.Dom)Dom).SetPath(selection.Substring(0, selection.LastIndexOf('/')));
				Selection.Reset();
			}

			return Result.EmptyResult(this);
		}

		private void MoveComponent(IComponent component, Guid folder)
		{
			Tenant.GetService<IDesignService>().Components.Update(component.Token, component.Name, folder);
		}

		private void MoveFolder(IFolder f, Guid folder)
		{
			Tenant.GetService<IDesignService>().Components.UpdateFolder(f.MicroService, f.Token, f.Name, folder);
		}

		public override IDesignerActionResult Action(JObject data)
		{
			var action = data.Optional("action", string.Empty);

			if (string.IsNullOrWhiteSpace(action))
				throw IdeException.ExpectedParameter(this, 0, "action");

			if (string.Compare(action, "clone", true) == 0)
				CloneComponent(data);

			return base.Action(data);
		}

		private void CloneComponent(JObject data)
		{
			var folder = data.Optional("folder", Guid.Empty);
			var component = data.Required<Guid>("component");

			Context.Tenant.GetService<IDesignService>().Components.Clone(component, MicroService.Token, folder);
		}
	}
}