using System;
using System.Collections.Generic;

namespace TomPIT.Ide.VersionControl
{
	public interface IChangeElement
	{
		Guid Id { get; }
		string Name { get; }
		List<IChangeElement> Elements { get; }
		IChangeBlob Blob { get; }
	}
}
