using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.ComponentModel.Resources;
using TomPIT.Designers;
using TomPIT.Dom;

namespace TomPIT.Development.Designers
{
	public class StringTableDesigner : DomDesigner<Dom.Element>
	{
		public StringTableDesigner(Element element) : base(element)
		{
		}

		public override object ViewModel => this;
		public override string View => "~/Views/Ide/Designers/StringTable.cshtml";

		public IStringTable StringTable { get { return Component as IStringTable; } }

		public JArray Items
		{
			get
			{
				var r = new JArray();

				foreach (var str in StringTable.Strings)
				{
					r.Add(new JObject
					{
						{"id", str.Id.ToString() },
						{ "key", str.Key },
						{"defaultValue", str.DefaultValue },
						{"isLocalizable", str.IsLocalizable }
					});
				}

				return r;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "add", true) == 0)
				return Add(data);
			else if (string.Compare(action, "update", true) == 0)
				return Update(data);
			else if (string.Compare(action, "delete", true) == 0)
				return Delete(data);

			return base.OnAction(data, action);
		}

		protected virtual IDesignerActionResult Add(JObject data)
		{
			return Result.EmptyResult(ViewModel);
		}

		protected virtual IDesignerActionResult Update(JObject data)
		{
			return Result.EmptyResult(ViewModel);
		}

		protected virtual IDesignerActionResult Delete(JObject data)
		{
			return Result.EmptyResult(ViewModel);
		}
	}
}
