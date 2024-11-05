using System;

namespace TomPIT.Proxy;
public interface ISourceFileInfo
{
	string FileName { get; }
	Guid Token { get; }
	int Size { get; }
	string ContentType { get; }
	string PrimaryKey { get; }
	Guid MicroService { get; }
	int Version { get; }
	int Type { get; }
	DateTime Modified { get; }
}
