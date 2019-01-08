using TomPIT.Annotations;
using TomPIT.UI;

namespace TomPIT.Application.UI
{
	[Create("Less", nameof(Name))]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("less")]
	public class LessFile : ThemeFile, ILessFile
	{
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.less", GetType().ShortName());

			return string.Format("{0}.less", Name);
		}
	}
}
