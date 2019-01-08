using TomPIT.Application.UI;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Design.Dom
{
	internal class LayoutElement : Element
	{
		public const string ElementId = "Layout";

		public LayoutElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = ElementId;
			Glyph = "fal fa-camera";
			Title = "Layout";
		}

		public override bool HasChildren => true;

		public override void LoadChildren()
		{
			LoadMasters();
			LoadPartials();
			LoadBranding();
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "MasterViews", true) == 0)
				LoadMasters();
			else if (string.Compare(id, "PartialViews", true) == 0)
				LoadPartials();
			else if (string.Compare(id, "Themes", true) == 0)
				LoadBranding();
		}

		private void LoadMasters()
		{
			Items.Add(new CategoryElement(Environment, this, MasterView.ComponentCategory, "MasterViews", "Master views", "fal fa-browser"));
		}

		private void LoadBranding()
		{
			Items.Add(new CategoryElement(Environment, this, Theme.ComponentCategory, "Themes", "Themes", "fal fa-paint-brush"));
		}

		private void LoadPartials()
		{
			Items.Add(new CategoryElement(Environment, this, Partial.ComponentCategory, "PartialViews", "Partial views", "fas fa-browser"));
		}
	}
}
