using TomPIT.Application.Data;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Dom
{
	internal class ComponentsElement : Element
	{
		public const string ElementId = "Components";

		public ComponentsElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = ElementId;
			Glyph = "fal fa-cog";
			Title = "Components";
		}

		public override bool HasChildren => true;

		public override void LoadChildren()
		{
			LoadDataManagement();
		}

		private void LoadDataManagement()
		{
			Items.Add(new CategoryElement(Environment, this, DataManagement.ComponentCategory, "Data management", "Data management", "fal fa-database"));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "Data management", true) == 0)
				LoadDataManagement();
		}
	}
}
