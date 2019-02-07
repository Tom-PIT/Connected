using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Models
{
	public class IdeModel : IdeModelBase
	{
		private IMicroServiceTemplate _template = null;

		private IMicroServiceTemplate Template
		{
			get
			{
				if (_template == null)
					_template = Connection.GetService<IMicroServiceTemplateService>().Select(MicroService.Template);

				return _template;
			}
		}

		public IMicroService MicroService { get; set; }

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
				return this.RouteUrl("ide", new { microService = MicroService.Url });
			}
		}

		protected override void OnDatabinding()
		{
			Title = MicroService.Name;

			Identity.SetContextId(MicroService.Token.ToString());
		}

		protected override IDesignerActionResult CreateItem(JObject data)
		{
			var item = data.Required<string>("item");
			var name = data.Required<string>("name");
			var descriptor = Selection.AddItems.FirstOrDefault(f => string.Compare(f.Id, item, true) == 0);

			var instance = descriptor.Type.CreateInstance();

			if (instance == null)
				throw IdeException.CannotCreateInstance(this, IdeEvents.IdeAction, descriptor.Type);

			var att = instance.GetType().FindAttribute<ComponentCreateHandlerAttribute>();

			if (att != null)
			{
				var handler = att.Type == null
					? Types.GetType(att.TypeName).CreateInstance<IComponentCreateHandler>()
					: att.Type.CreateInstance<IComponentCreateHandler>();

				if (handler != null)
					handler.InitializeNewComponent(instance);
			}

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
			var id = GetService<IComponentDevelopmentService>().Insert(ms.MicroService.Token, fs == null ? Guid.Empty : fs.Token, descriptor.Id.ToString(), name, descriptor.Type.TypeName());
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
			var id = GetService<IComponentDevelopmentService>().InsertFolder(ms.MicroService.Token, name, parent);
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
			GetService<IComponentDevelopmentService>().DeleteFolder(DomQuery.Closest<IMicroServiceScope>(Selection.Element).MicroService.Token, folder.Token);

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
			GetService<IComponentDevelopmentService>().Delete(component);

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
				var f = Connection.GetService<IComponentService>().SelectFolder(folder);
				/*
				 * it's a microservice not a folder
				 */
				if (f == null)
					folder = Guid.Empty;
			}

			var target = GetService<IComponentService>().SelectComponent(id);

			if (target != null)
				MoveComponent(target, folder);
			else
			{
				var targetFolder = GetService<IComponentService>().SelectFolder(id);

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
			GetService<IComponentDevelopmentService>().Update(component.Token, component.Name, folder);
		}

		private void MoveFolder(IFolder f, Guid folder)
		{
			GetService<IComponentDevelopmentService>().UpdateFolder(f.MicroService, f.Token, f.Name, folder);
		}
	}
}