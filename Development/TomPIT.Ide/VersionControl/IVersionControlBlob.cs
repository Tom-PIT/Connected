using System;

namespace TomPIT.Ide.VersionControl
{
	public interface IVersionControlBlob
	{
		Guid Token { get; set; }
		byte[] Content { get; set; }
	}
}
