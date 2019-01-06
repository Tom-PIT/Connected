using System;
using TomPIT.Environment;

namespace TomPIT.SysDb.Environment
{
	public interface IServerResourceGroup : IResourceGroup
	{
		Guid StorageProvider { get; }
		string ConnectionString { get; }
	}
}
