using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[Create("AssemblyUpload")]
	[DomDesigner("TomPIT.Application.Design.Designers.AssemblyUploadDesigner, TomPIT.Application.Design")]
	public class AssemblyUploadResource : ComponentConfiguration, IAssemblyUploadResource
	{
		public const string ComponentCategory = "Assembly";

		[Browsable(false)]
		public Guid Blob { get; set; }
		[Browsable(false)]
		public string FileName { get; set; }
	}
}
