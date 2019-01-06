using System;

namespace TomPIT.Globalization
{
	public class LanguageEventArgs : EventArgs
	{
		public LanguageEventArgs(Guid language)
		{
			Language = language;
		}

		public Guid Language { get; }
	}
}
