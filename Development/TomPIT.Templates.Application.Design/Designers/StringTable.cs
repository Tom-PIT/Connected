using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.Application.Resources;
using TomPIT.Design;
using TomPIT.Development.Designers;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Designers
{
	internal class StringTable : StringTableDesigner
	{
		public StringTable(Element element) : base(element)
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

			Connection.GetService<IComponentDevelopmentService>().Update(StringTable);

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

			Connection.GetService<IComponentDevelopmentService>().Update(StringTable);

			return Result.EmptyResult(ViewModel);
		}

		protected override IDesignerActionResult Delete(JObject data)
		{
			var id = data.Required<Guid>("id");

			var existing = StringTable.Strings.FirstOrDefault(f => f.Id == id);

			if (existing != null)
				StringTable.Strings.Remove(existing);

			Connection.GetService<IComponentDevelopmentService>().Update(StringTable);

			return Result.EmptyResult(ViewModel);
		}
	}
}
