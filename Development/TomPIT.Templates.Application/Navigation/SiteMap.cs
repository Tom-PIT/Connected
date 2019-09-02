using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Navigation;

namespace TomPIT.Application.Navigation
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class SiteMap : ComponentConfiguration, ISiteMapConfiguration
	{
		public const string ComponentCategory = "SiteMap";

		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
