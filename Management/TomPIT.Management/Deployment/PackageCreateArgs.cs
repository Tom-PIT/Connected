using System;
using TomPIT.Connectivity;

namespace TomPIT.Deployment
{
	public delegate void PackageProcessHandler(PackageProcessArgs e);

	public class PackageCreateArgs : EventArgs
	{
		public PackageCreateArgs(ISysConnection connection, Guid microService, IPackageMetaData metaData, PackageProcessHandler processCallback)
		{
			Connection = connection;
			MicroService = microService;
			MetaData = metaData;
			Callback = processCallback;
		}

		public ISysConnection Connection { get; }
		public Guid MicroService { get; }
		public IPackageMetaData MetaData { get; }
		public PackageProcessHandler Callback { get; }
	}
}
