using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Design;
using TomPIT.Globalization;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Designers.Toolbar;
using TomPIT.Management.Dom;
using TomPIT.Management.Globalization;
using TomPIT.Management.Items;

namespace TomPIT.Management.Designers
{
	internal class LanguagesDesigner : CollectionDesigner<LanguagesElement>
	{
		public LanguagesDesigner(LanguagesElement element) : base(element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, LanguagesCollection.Token, true) == 0)
				return CreateNode();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var user = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Environment.Context.Tenant.GetService<IGlobalizationManagementService>().DeleteLanguage(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateNode()
		{
			var existing = Owner.Existing;
			var name = Environment.Context.Tenant.GetService<INamingService>().Create("Language", existing.Select(f => f.Name), true);
			var id = Environment.Context.Tenant.GetService<IGlobalizationManagementService>().InsertLanguage(name, 0, TomPIT.Globalization.LanguageStatus.Hidden, string.Empty);
			var language = Environment.Context.Tenant.GetService<ILanguageService>().Select(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.LanguageCreateSuccess, name);
			r.Title = SR.DevCreateLanguage;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), language.Token);

			return r;
		}

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId && action.Id != Ide.Designers.Toolbar.Clear.ActionId;
		}

		public override bool SupportsReorder
		{
			get { return false; }
		}
	}
}
