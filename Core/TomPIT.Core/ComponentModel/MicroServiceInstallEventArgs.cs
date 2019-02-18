using System;

namespace TomPIT.ComponentModel
{
	public class MicroServiceInstallEventArgs : MicroServiceEventArgs
	{
		public MicroServiceInstallEventArgs(Guid microService, bool success) : base(microService)
		{
			Success = success;
		}

		public bool Success { get; }
	}
}
