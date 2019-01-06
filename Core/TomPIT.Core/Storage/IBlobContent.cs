using System;

namespace TomPIT.Storage
{
	public interface IBlobContent
	{
		Guid Blob { get; }
		byte[] Content { get; }
	}
}
