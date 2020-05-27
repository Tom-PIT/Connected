using System;
using System.Collections.Generic;

namespace TomPIT.Ide.VersionControl
{
	public interface IChangeMicroService
	{
		string Name { get; }
		Guid Id { get; }

		List<IChangeComponent> Components { get; }
		List<IChangeFolder> Folders { get; }
	}
}
