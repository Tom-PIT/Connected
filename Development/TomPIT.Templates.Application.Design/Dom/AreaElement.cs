using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Design.Dom
{
	internal class AreaElement : ComponentElement, IComponentScope
	{
		public AreaElement(IEnvironment environment, IDomElement parent, IComponent component) : base(environment, parent, component)
		{
		}

		public override void LoadChildren()
		{
			Items.Add(new CategoryElement(Environment, this, "View", "Views", "Views", "fal fa-browser"));

			base.LoadChildren();
		}

		public override bool HasChildren => true;

		IComponent IComponentScope.Component => Target as IComponent;

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "Views", true) == 0)
				Items.Add(new CategoryElement(Environment, this, "View", "Views", "Views", "fal fa-browser"));
			else
				base.LoadChildren(id);
		}
	}
}
