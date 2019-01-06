using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[Create("Javascript", nameof(Name))]
	[DomDesigner("TomPIT.Design.TemplateDesigner, TomPIT.Ide")]
	[Syntax("javascript")]
	public class ScriptCodeSource : ScriptSource, IText, IScriptCodeSource
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
