using System;

namespace TomPIT.Ide.Design
{
	public interface IComponentImageBlob
	{
		Guid Token { get; }
		string ContentType { get; }
		string FileName { get; }
		string PrimaryKey { get; }
		string Topic { get; }
		int Type { get; }
		int Version { get; }
		byte[] Content { get; }
	}
}
