using System;

namespace TomPIT.ComponentModel
{
	public class FeatureEventArgs : EventArgs
	{
		public FeatureEventArgs(Guid microService, Guid feature)
		{
			MicroService = microService;
			Feature = feature;
		}

		public Guid MicroService { get; }
		public Guid Feature { get; }
	}
}
