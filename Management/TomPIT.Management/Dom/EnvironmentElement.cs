using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class EnvironmentElement : Element
	{
		public const string DomId = "Environment";

		public EnvironmentElement(IEnvironment environment) : base(environment, null)
		{
			Id = DomId;
			Glyph = "fal fa-folder";
			Title = "Environment";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override bool HasChildren { get { return true; } }

		public override void LoadChildren()
		{
			Items.Add(new EndpointsElement(Environment, this));
			Items.Add(new EnvironmentUnitsElement(Environment, this));
			Items.Add(new SettingsElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, EndpointsElement.DomId, true) == 0)
				Items.Add(new EndpointsElement(Environment, this));
			else if (string.Compare(id, EnvironmentUnitsElement.DomId, true) == 0)
				Items.Add(new EnvironmentUnitsElement(Environment, this));
			else if (string.Compare(id, SettingsElement.ElementId, true) == 0)
				Items.Add(new SettingsElement(Environment, this));
		}
	}
}
