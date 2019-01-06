using System;

namespace TomPIT.Environment
{
	public class EnvironmentUnitEventArgs : EventArgs
	{
		public EnvironmentUnitEventArgs(Guid environmentUnit)
		{
			EnvironmentUnit = environmentUnit;
		}

		public Guid EnvironmentUnit { get; }
	}
}
