using TomPIT.Annotations;

namespace TomPIT.Application.UI
{
	[Create("Stylesheet", nameof(Name))]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("css")]
	public class CssFile : ThemeFile
	{
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.css", GetType().ShortName());

			return string.Format("{0}.css", Name);
		}

	}
}
