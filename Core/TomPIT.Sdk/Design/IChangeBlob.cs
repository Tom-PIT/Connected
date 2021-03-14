using System;

namespace TomPIT.Design
{
	public interface IChangeBlob
	{
		Guid Token { get; set; }
		string ContentType { get; set; }
		byte[] Content { get; set; }
		string Syntax { get; set; }
		string FileName { get; set; }
		bool HasChanged { get; set; }
	}
}
