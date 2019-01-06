using System;

namespace TomPIT.ComponentModel.Resources
{
	public interface IUploadResource : IElement
	{
		Guid Blob { get; set; }
		string FileName { get; set; }
	}
}
