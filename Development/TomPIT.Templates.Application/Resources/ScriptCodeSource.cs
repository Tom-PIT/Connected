using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Resources
{
	[Create(DesignUtils.Javascript, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Javascript)]
	public class ScriptCodeSource : ScriptSource, IText, IScriptCodeSource
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
