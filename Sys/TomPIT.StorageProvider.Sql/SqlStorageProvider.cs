using System;
using TomPIT.Api.Storage;

namespace TomPIT.StorageProvider.Sql
{
	public class SqlStorageProvider : IStorageProvider
	{
		private Lazy<Blobs> _blobs = new Lazy<Blobs>();

		public Guid Token { get { return new Guid("E8BC6674718241D0ADA31835BDA71E36"); } }

		public IBlobProvider Blobs { get { return _blobs.Value; } }

		public string Name => "Microsoft SQL Server";
	}
}
