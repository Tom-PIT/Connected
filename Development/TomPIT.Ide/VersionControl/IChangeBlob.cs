using System;

namespace TomPIT.Ide.VersionControl
{
	public interface IChangeBlob
	{
		Guid Token { get; set; }
		string ContentType { get; set; }
		byte[] Content { get; set; }
		string Syntax { get; set; }
		string FileName { get; set; }
	}
}
