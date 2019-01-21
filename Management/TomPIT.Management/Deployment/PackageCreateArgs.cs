using System;
using TomPIT.Connectivity;

namespace TomPIT.Deployment
{
	public delegate void PackageProcessHandler(PackageProcessArgs e);

	public class PackageCreateArgs : EventArgs
	{
		public PackageCreateArgs(ISysConnection connection, Guid microService, PackageMetaData metaData, PackageProcessHandler processCallback)
		{
			Connection = connection;
			MicroService = microService;
			MetaData = metaData;
			Callback = processCallback;
		}

		public ISysConnection Connection { get; }
		public Guid MicroService { get; }
		public PackageMetaData MetaData { get; }
		public PackageProcessHandler Callback { get; }
	}
}
