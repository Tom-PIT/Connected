using System;
using TomPIT.Proxy;

namespace TomPIT.ComponentModel;
internal class SourceTextCacheEntry
{
	private ISourceFileInfo? _info = null;

	public Guid MicroService { get; set; }
	public Guid Token { get; set; }
	public int Type { get; set; }
	public string? Text { get; set; }
	public ISourceFileInfo? Info => _info ??= Instance.SysProxy.SourceFiles.Select(Token, Type);
}
