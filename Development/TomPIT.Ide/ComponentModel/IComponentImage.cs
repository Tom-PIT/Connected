using System;
using System.Collections.Generic;

namespace TomPIT.Ide.ComponentModel
{
	public interface IComponentImage
	{
		Guid Token { get; }
		string Name { get; }
		string Category { get; }
		Guid Folder { get; }
		Guid MicroService { get; }
		Guid RuntimeConfiguration { get; }
		string Type { get; }
		IComponentImageBlob Configuration { get; set; }

		List<IComponentImageBlob> Dependencies { get; }
	}
}
