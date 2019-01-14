using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.Configuration;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	internal class SettingsDesigner : ListDesigner<SettingsElement>, IDesignerSelectionProvider
	{
		private ISetting _setting = null;

		public SettingsDesigner(IEnvironment environment, SettingsElement element) : base(environment, element)
		{

		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", string.Empty);

			if (string.IsNullOrWhiteSpace(id))
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var setting = Owner.Existing.FirstOrDefault(f => string.Compare(f.Name, id, true) == 0);
			var rg = DomQuery.Closest<IResourceGroupScope>(Owner);
			Connection.GetService<ISettingManagementService>().Delete(rg == null ? Guid.Empty : rg.ResourceGroup.Token, setting.Name);

			return Result.SectionResult(this, EnvironmentSection.Designer);
		}

		protected override IDesignerActionResult OnCreateComponent(object component)
		{
			var s = component as ISetting;
			var rg = DomQuery.Closest<IResourceGroupScope>(Owner);
			Connection.GetService<ISettingManagementService>().Update(rg == null ? Guid.Empty : rg.ResourceGroup.Token, s.Name, s.Value, true, s.DataType, s.Tags);
			s = Connection.GetService<ISettingService>().Select(rg == null ? Guid.Empty : rg.ResourceGroup.Token, s.Name);

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
			return action.Id != Undo.ActionId && action.Id != Actions.Clear.ActionId;
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

					var rg = DomQuery.Closest<IResourceGroupScope>(Element);

					_setting = Connection.GetService<ISettingService>().Select(rg == null ? Guid.Empty : rg.ResourceGroup.Token, SelectionId);
				}

				return _setting;
			}
		}

		public string SelectionId { get { return Environment.RequestBody.Optional("designerSelectionId", string.Empty); } }

		public override string ItemTemplateView => "~/Views/Ide/ItemTemplates/Setting.cshtml";
	}
}
