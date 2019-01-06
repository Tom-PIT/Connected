using System;

namespace TomPIT.Environment
{
	public class ResourceGroupEventArgs : EventArgs
	{
		public ResourceGroupEventArgs(Guid resourceGroup)
		{
			ResourceGroup = resourceGroup;
		}

		public Guid ResourceGroup { get; }
	}
}
