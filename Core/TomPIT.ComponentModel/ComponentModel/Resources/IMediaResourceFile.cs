using System;

namespace TomPIT.ComponentModel.Resources
{
	public interface IMediaResourceFile : IConfigurationElement, IUploadResource
	{
		long Size { get; set; }
		DateTime Modified { get; set; }
		Guid Thumb { get; set; }
	}
}
