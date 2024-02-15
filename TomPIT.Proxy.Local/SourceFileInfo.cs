using System;

namespace TomPIT.Proxy.Local;
internal sealed class SourceFileInfo : ISourceFileInfo
{
	public string FileName { get; set; }

	public Guid Token { get; set; }

	public int Size { get; set; }

	public string ContentType { get; set; }

	public string PrimaryKey { get; set; }

	public Guid MicroService { get; set; }

	public int Version { get; set; }

	public int Type { get; set; }

	public DateTime Modified { get; set; }
}
