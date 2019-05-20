using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.UI;

namespace TomPIT.Application.UI
{
	[Create("Less", nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax("less")]
	public class LessIncludeFile : LessFile, ILessIncludeFile
	{
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.include", GetType().ShortName());

			return string.Format("{0}.include", Name);
		}
	}
}
