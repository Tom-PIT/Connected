using System;

namespace TomPIT.ComponentModel.Resources
{
	public interface IMediaResourceFile : IElement, IUploadResource
	{
		long Size { get; set; }
		DateTime Modified { get; set; }
		Guid Thumb { get; set; }
	}
}
