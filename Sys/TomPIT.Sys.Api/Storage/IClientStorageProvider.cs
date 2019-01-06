using System;

namespace TomPIT.Api.Storage
{
	public interface IClientStorageProvider
	{
		Guid Token { get; }
		string Name { get; }
	}
}
