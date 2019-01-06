using System;
using TomPIT.Api.Storage;

namespace TomPIT.StorageProvider.Sql
{
	public class SqlStorageProvider : IStorageProvider
	{
		private Lazy<Blobs> _blobs = new Lazy<Blobs>();
		private Lazy<ReliableMessaging> _messaging = new Lazy<ReliableMessaging>();
		private Lazy<Queue> _queue = new Lazy<Queue>();

		public Guid Token { get { return new Guid("E8BC6674718241D0ADA31835BDA71E36"); } }

		public IBlobProvider Blobs { get { return _blobs.Value; } }
		public IReliableMessagingProvider Messaging { get { return _messaging.Value; } }
		public IQueueProvider Queue { get { return _queue.Value; } }

		public string Name => "Microsoft SQL Server";
	}
}
