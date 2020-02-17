using System;

namespace TomPIT.Cdn
{
	public class DomainEventArgs : EventArgs
	{
		public DomainEventArgs(string domain)
		{
			Domain = domain;
		}

		public string Domain { get; }
	}
}
