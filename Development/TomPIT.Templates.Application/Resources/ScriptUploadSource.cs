using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[Create("ScriptUpload")]
	[DomDesigner("TomPIT.Application.Design.ScriptUploadDesigner, TomPIT.Templates.Application")]
	public class ScriptUploadSource : ScriptSource, IScriptUploadSource
	{
		[Browsable(false)]
		public Guid Blob { get; set; }
		[Browsable(false)]
		public string FileName { get; set; }
	}
}
