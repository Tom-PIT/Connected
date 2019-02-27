using TomPIT.Development.Designers;

namespace TomPIT.Dom
{
	internal class VersionControlElement : Element
	{
		public const string ElementId = "VersionControl";

		private IDomDesigner _designer = null;

		public VersionControlElement(IDomElement parent) : base(parent)
		{
			((Behavior)Behavior).Static = true;

			Id = ElementId;
			Title = "Version control";
			Glyph = "fal fa-code-branch";
			Verbs.Clear();
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new VersionControlChangesDesigner(this);

				return _designer;
			}
		}
	}
}
