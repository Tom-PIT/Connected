using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Resources;
using TomPIT.Design.Ide.Designers;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;

namespace TomPIT.Development.Designers
{
	public class StringTableDesigner : DomDesigner<DomElement>
	{
		public StringTableDesigner(DomElement element) : base(element)
		{
		}

		public override object ViewModel => this;
		public override string View => "~/Views/Ide/Designers/StringTable.cshtml";

		public IStringTableConfiguration StringTable { get { return Component as IStringTableConfiguration; } }

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
