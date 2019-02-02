using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	internal class ComponentsDesigner : CollectionDesigner<IComponentsElement>
	{
		public ComponentsDesigner(IEnvironment environment, IComponentsElement element) : base(environment, element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			var target = Descriptors.FirstOrDefault(f => string.Compare(f.Id, d.Id, true) == 0);

			if (target == null)
				throw new IdeException(SR.ErrItemDescriptorNotFound);

			return CreateComponent(target);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var scope = DomQuery.Closest<IFolderScope>(Owner);
			var id = data.Optional("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var component = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Connection.GetService<IComponentDevelopmentService>().Delete(component.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		protected override IDesignerActionResult Clear(JObject data)
		{
			var scope = DomQuery.Closest<IFolderScope>(Owner);

			foreach (var i in Owner.Existing)
				Connection.GetService<IComponentDevelopmentService>().Delete(i.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateComponent(IItemDescriptor descriptor)
		{
			var shortName = descriptor.Type.ShortName();
			var att = descriptor.Type.FindAttribute<CreateAttribute>();
			var name = Connection.GetService<IComponentDevelopmentService>().CreateName(Element.MicroService(), Owner.Category, att == null ? descriptor.Type.ShortName() : att.Prefix);

			var scope = DomQuery.Closest<IFolderScope>(Owner);

			var id = Connection.GetService<IComponentDevelopmentService>().Insert(Element.MicroService(), scope == null ? Guid.Empty : scope.Folder.Token, Owner.Category, name, descriptor.Type.TypeName());
			var component = Connection.GetService<IComponentService>().SelectComponent(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.DevColCreateSuccessful, name);
			r.Title = SR.DevAddComponent;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), component.Token);

			return r;
		}

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId;
		}

		public override bool SupportsReorder
		{
			get { return false; }
		}

		public override List<IItemDescriptor> Descriptors => Owner.Descriptors;
	}
}
