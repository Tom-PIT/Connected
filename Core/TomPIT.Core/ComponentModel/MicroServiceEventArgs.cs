using System;

namespace TomPIT.ComponentModel
{
	public class MicroServiceEventArgs : EventArgs
	{
		public MicroServiceEventArgs()
		{

		}
		public MicroServiceEventArgs(Guid microService)
		{
			MicroService = microService;
		}

		public Guid MicroService { get; set; }
	}
}
