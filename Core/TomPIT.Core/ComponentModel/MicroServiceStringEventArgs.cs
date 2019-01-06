using System;

namespace TomPIT.ComponentModel
{
	public class MicroServiceStringEventArgs : EventArgs
	{
		public MicroServiceStringEventArgs(Guid microService, Guid language, Guid element, string property)
		{
			MicroService = microService;
			Language = language;
			Element = element;
			Property = property;
		}

		public Guid MicroService { get; }
		public Guid Language { get; }
		public Guid Element { get; }
		public string Property { get; }
	}
}
