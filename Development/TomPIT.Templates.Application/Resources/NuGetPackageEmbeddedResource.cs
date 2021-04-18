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
	[Create(DesignUtils.Package)]
	[DomDesigner(DesignUtils.NuGetPackageEmbeddedDesigner)]
	public class NuGetPackageEmbeddedResource : ComponentConfiguration, INuGetPackageEmbeddedResource, IExternalResourceElement
	{
		[Browsable(false)]
		public Guid Blob { get; set; }
		[Browsable(false)]
		public string FileName { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[Browsable(false)]
		public string Id => FileName;

		[Browsable(false)]
		public string Version { get; set; }

		public List<Guid> QueryResources()
		{
			return new List<Guid> { Blob };
		}

		public void Clean(Guid resource)
		{
			if (Blob != Guid.Empty)
				MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Delete(Blob);
		}

		public void Reset(Guid existingValue, Guid newValue)
		{
			Blob = newValue;
		}
	}
}
