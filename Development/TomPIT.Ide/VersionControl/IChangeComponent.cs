using System;

namespace TomPIT.Ide.VersionControl
{
	public interface IChangeComponent : IChangeElement
	{
		Guid Folder { get; }
		Guid Microservice { get; }
		byte[] Configuration { get; }
		byte[] RuntimeConfiguration { get; }
		string Error { get; }
	}
}
