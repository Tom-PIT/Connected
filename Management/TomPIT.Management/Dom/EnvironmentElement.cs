using TomPIT.Design.Ide;
using TomPIT.Ide.Dom;

namespace TomPIT.Management.Dom
{
	public class EnvironmentElement : DomElement
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
			Items.Add(new SettingsElement(this));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, SettingsElement.ElementId, true) == 0)
				Items.Add(new SettingsElement(this));
		}
	}
}
