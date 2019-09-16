using System;
using TomPIT.Collections;

namespace TomPIT.ComponentModel.Resources
{
	public interface IMediaResourceFolder : IElement
	{
		ListItems<IMediaResourceFolder> Folders { get; }
		ListItems<IMediaResourceFile> Files { get; }

		string Name { get; set; }
		DateTime Modified { get; set; }
	}
}
