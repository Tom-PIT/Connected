using System;

namespace TomPIT.ComponentModel
{
	public class SourceTextChangedEventArgs : EventArgs
	{
		public SourceTextChangedEventArgs()
		{

		}
		public SourceTextChangedEventArgs(Guid microService, Guid configuration, Guid token, int type)
		{
			Token = token;
			MicroService = microService;
			Type = type;
			Configuration = configuration;
		}

		public int Type { get; set; }
		public Guid Token { get; set; }
		public Guid Configuration { get; set; }
		public Guid MicroService { get; set; }
	}
}
