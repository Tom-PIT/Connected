using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Development.Designers;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Resources;

namespace TomPIT.MicroServices.Design.Designers
{
	internal class StringTable : StringTableDesigner
	{
		public StringTable(DomElement element) : base(element)
		{
		}

		protected override IDesignerActionResult Add(JObject data)
		{
			var existing = new StringResource
			{
				Key = data.Required<string>("key"),
				DefaultValue = data.Optional("defaultValue", string.Empty),
				IsLocalizable = data.Optional("isLocalizable", true)
			};

			StringTable.Strings.Add(existing);

			Environment.Context.Tenant.GetService<IComponentDevelopmentService>().Update(StringTable);

			return Result.JsonResult(ViewModel, existing.Id);
		}

		protected override IDesignerActionResult Update(JObject data)
		{
			var id = data.Required<Guid>("id");

			var existing = StringTable.Strings.FirstOrDefault(f => f.Id == id) as StringResource;

			if (existing != null)
			{
				existing.Key = data.Optional("key", existing.Key);
				existing.DefaultValue = data.Optional("defaultValue", existing.DefaultValue);
				existing.IsLocalizable = data.Optional("isLocalizable", existing.IsLocalizable);
			}

			Environment.Context.Tenant.GetService<IComponentDevelopmentService>().Update(StringTable);

			return Result.EmptyResult(ViewModel);
		}

		protected override IDesignerActionResult Delete(JObject data)
		{
			var id = data.Required<Guid>("id");

			var existing = StringTable.Strings.FirstOrDefault(f => f.Id == id);

			if (existing != null)
				StringTable.Strings.Remove(existing);

			Environment.Context.Tenant.GetService<IComponentDevelopmentService>().Update(StringTable);

			return Result.EmptyResult(ViewModel);
		}
	}
}
