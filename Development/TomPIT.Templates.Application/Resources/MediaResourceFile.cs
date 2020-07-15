using System;
using System.Collections.Generic;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.MicroServices.Resources
{
	[Create(DesignUtils.MediaFile)]
	[DomDesigner(DesignUtils.MediaResourceFileUploadDesigner)]
	public class MediaResourceFile : ConfigurationElement, IMediaResourceFile, IExternalResourceElement
	{
		[Browsable(false)]
		public Guid Blob { get; set; }
		[Browsable(false)]
		public Guid Thumb { get; set; }
		[Browsable(false)]
		public string FileName { get; set; }

		[Browsable(false)]
		public long Size { get; set; }

		[Browsable(false)]
		public DateTime Modified { get; set; }

		public List<Guid> QueryResources()
		{
			return new List<Guid> { Blob, Thumb };
		}

		public void Delete(Guid resource)
		{
			if (Blob != Guid.Empty)
				MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Delete(Blob);
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(FileName)
				? base.ToString()
				: FileName;
		}

		public void Clean(Guid resource)
		{
			if (Blob != Guid.Empty)
				MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Delete(Blob);

			if (Thumb != Guid.Empty)
				MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Delete(Thumb);
		}
	}
}
