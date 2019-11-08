using TomPIT.Development.Designers;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;

namespace TomPIT.Dom
{
	internal class VersionControlElement : DomElement
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
