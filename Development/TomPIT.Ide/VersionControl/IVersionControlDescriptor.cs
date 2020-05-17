using System;
using System.Collections.Generic;

namespace TomPIT.Ide.VersionControl
{
	public interface IVersionControlDescriptor
	{
		Guid Id { get; }
		string Name { get; }
		string Syntax { get; }
		Guid Folder { get; }
		Guid Blob { get; }
		Guid Microservice { get; }
		Guid Component { get; }
		List<IVersionControlDescriptor> Items { get; }
		List<IVersionControlBlob> Blobs { get; }
	}
}
