using System;
using System.Collections.Generic;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Storage;

namespace TomPIT.Application.Resources
{
	[Create("AssemblyUpload")]
	[DomDesigner("TomPIT.Application.Design.Designers.AssemblyUploadDesigner, TomPIT.Application.Design")]
	public class AssemblyUploadResource : ComponentConfiguration, IAssemblyUploadResource, IExternalResourceElement
	{
		public const string ComponentCategory = "Assembly";

		[Browsable(false)]
		public Guid Blob { get; set; }
		[Browsable(false)]
		public string FileName { get; set; }

		public List<Guid> QueryResources()
		{
			return new List<Guid> { Blob };
		}

		public void Delete(ExternalResourceDeleteArgs e)
		{
			if (Blob != Guid.Empty)
				e.Connection.GetService<IStorageService>().Delete(Blob);
		}
	}
}
