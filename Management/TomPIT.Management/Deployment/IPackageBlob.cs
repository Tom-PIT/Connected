using System;

namespace TomPIT.Deployment
{
	public interface IPackageBlob
	{
		string Content { get; }
		string FileName { get; }
		Guid Token { get; }
		int Type { get; }
		string ContentType { get; }
		string PrimaryKey { get; }
		Guid MicroService { get; }
		int Version { get; }
		string Topic { get; }
	}
}
