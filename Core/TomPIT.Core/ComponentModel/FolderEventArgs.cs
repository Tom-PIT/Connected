using System;

namespace TomPIT.ComponentModel
{
	public class FolderEventArgs : EventArgs
	{
		public FolderEventArgs(Guid microService, Guid folder)
		{
			MicroService = microService;
			Folder = folder;
		}

		public Guid MicroService { get; }
		public Guid Folder { get; }
	}
}
