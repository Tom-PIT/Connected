using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.UI;

namespace TomPIT.Reporting.UI
{
	[Create("Less", nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
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
