using TomPIT.Application.Data;
using TomPIT.Application.Workers;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Design.Dom
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
			LoadWorkers();
		}

		private void LoadDataManagement()
		{
			Items.Add(new CategoryElement(Environment, this, DataManagement.ComponentCategory, "Data management", "Data management", "fal fa-database"));
		}

		private void LoadWorkers()
		{
			Items.Add(new CategoryElement(Environment, this, HostedWorker.ComponentCategory, "Workers", "Workers", "fal fa-folder"));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "Data management", true) == 0)
				LoadDataManagement();
			else if (string.Compare(id, "Workers", true) == 0)
				LoadWorkers();
		}
	}
}
