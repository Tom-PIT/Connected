using System;
using System.Collections.Generic;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[Create("ScriptUpload")]
	[DomDesigner("TomPIT.Application.Design.Designers.ScriptUploadDesigner, TomPIT.Application.Design")]
	public class ScriptUploadSource : ScriptSource, IScriptUploadSource, IExternalResourceElement
	{
		[Browsable(false)]
		public Guid Blob { get; set; }
		[Browsable(false)]
		public string FileName { get; set; }

		public List<Guid> QueryResources()
		{
			return new List<Guid> { Blob };
		}
	}
}
