using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Reporting
{
	[Create("Javascript", nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax("javascript")]
	public class Javascript : Text
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.js", GetType().ShortName());

			return string.Format("{0}.js", Name);
		}
	}
}
