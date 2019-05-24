using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.UI;

namespace TomPIT.Reporting.UI
{
	[Create("Stylesheet", nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax("css")]
	public class CssFile : ThemeFile, ICssFile
	{
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.css", GetType().ShortName());

			return string.Format("{0}.css", Name);
		}

	}
}
