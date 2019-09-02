using System;

namespace TomPIT.Storage
{
	public enum StoragePolicy
	{
		Singleton = 1,
		Multiple = 2,
		Extended = 3
	}

	public interface IBlob
	{
		Guid ResourceGroup { get; }
		string FileName { get; }
		Guid Token { get; }
		int Size { get; }
		string ContentType { get; }
		string PrimaryKey { get; }
		Guid MicroService { get; }
		string Draft { get; }
		int Version { get; }
		int Type { get; }
		string Topic { get; }
		DateTime Modified { get; }
	}
}
