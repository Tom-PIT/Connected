using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Configuration;
using TomPIT.Design.Ide.Designers;
using TomPIT.Ide;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Designers.Toolbar;
using TomPIT.Ide.Environment.Providers;
using TomPIT.Management.Configuration;
using TomPIT.Management.Dom;

namespace TomPIT.Management.Designers
{
	internal class SettingsDesigner : ListDesigner<SettingsElement>, IDesignerSelectionProvider
	{
		private ISetting _setting = null;

		public SettingsDesigner(SettingsElement element) : base(element)
		{

		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", string.Empty);

			if (string.IsNullOrWhiteSpace(id))
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var setting = Owner.Existing.FirstOrDefault(f => string.Compare(f.Name, id, true) == 0);
			Environment.Context.Tenant.GetService<ISettingManagementService>().Delete(setting.Name, null, null, null);

			return Result.SectionResult(this, EnvironmentSection.Designer);
		}

		protected override IDesignerActionResult OnCreateComponent(object component)
		{
			var s = component as ISetting;
			IdeExtensions.ProcessComponentCreated(Environment.Context, component);
			Environment.Context.Tenant.GetService<ISettingService>().Update(s.Name, null, null, null, s.Value);
			s = Environment.Context.Tenant.GetService<ISettingService>().Select(s.Name, null, null, null);


			var r = Result.SectionResult(this, EnvironmentSection.Designer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format("Setting '{0}' successfully created.", s.Name);
			r.Title = "Add setting";
			r.ExplorerPath = DomQuery.Path(Element);
			r.Data = new JObject
			{
				{"id", s.Name }
			};

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

		public object Value
		{
			get
			{
				if (_setting == null)
				{
					if (string.IsNullOrWhiteSpace(SelectionId))
						return null;

					_setting = Environment.Context.Tenant.GetService<ISettingService>().Select(SelectionId, null, null, null);
				}

				return _setting;
			}
		}

		public string SelectionId { get { return Environment.RequestBody.Optional("designerSelectionId", string.Empty); } }

		public override string ItemTemplateView => "~/Views/Ide/ItemTemplates/Setting.cshtml";
	}
}
