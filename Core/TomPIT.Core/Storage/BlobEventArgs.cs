using System;

namespace TomPIT.Storage
{
	public class BlobEventArgs : EventArgs
	{
		public BlobEventArgs(Guid microService, Guid blob, int type, string primaryKey)
		{
			MicroService = microService;
			Blob = blob;
			PrimaryKey = primaryKey;
			Type = type;
		}

		public Guid MicroService { get; }
		public Guid Blob { get; }
		public int Type { get; }
		public string PrimaryKey { get; }
	}
}
