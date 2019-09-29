using System;

namespace TomPIT.Storage
{
	public class BlobEventArgs : EventArgs
	{
		public BlobEventArgs()
		{

		}
		public BlobEventArgs(Guid microService, Guid blob, int type, string primaryKey)
		{
			MicroService = microService;
			Blob = blob;
			PrimaryKey = primaryKey;
			Type = type;
		}

		public Guid MicroService { get; set; }
		public Guid Blob { get; set; }
		public int Type { get; set; }
		public string PrimaryKey { get; set; }
	}
}
