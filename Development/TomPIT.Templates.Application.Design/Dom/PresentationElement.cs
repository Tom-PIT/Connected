using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Design.Dom
{
	internal class PresentationElement : Element
	{
		public const string ElementId = "Presentation";

		public PresentationElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = ElementId;
			Glyph = "fal fa-tv";
			Title = "Presentation";
		}

		public override bool HasChildren => true;

		public override void LoadChildren()
		{
			LoadAreas();
			LoadLayout();
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "Areas", true) == 0)
				LoadAreas();
			else if (string.Compare(id, LayoutElement.ElementId, true) == 0)
				LoadLayout();
		}

		private void LoadLayout()
		{
			Items.Add(new LayoutElement(Environment, this));
		}


		private void LoadAreas()
		{
			Items.Add(new CategoryElement(Environment, this, "Area", "Areas", "Areas", "fal fa-expand-wide"));
		}
	}
}
