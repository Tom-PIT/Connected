using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Management.Dom
{
	public class GlobalizationElement : Element
	{
		public const string DomId = "Globalization";

		public GlobalizationElement(IEnvironment environment) : base(environment, null)
		{
			Id = DomId;
			Glyph = "fal fa-globe";
			Title = "Globalization";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override bool HasChildren { get { return true; } }

		public override void LoadChildren()
		{
			Items.Add(new LanguagesElement(Environment, this));
			Items.Add(new TranslationElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, LanguagesElement.FolderId, true) == 0)
				Items.Add(new LanguagesElement(Environment, this));
			else if (string.Compare(id, TranslationElement.FolderId, true) == 0)
				Items.Add(new TranslationElement(Environment, this));
		}
	}
}
