using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Globalization
{
	public interface ILanguageNotification
	{
		void NotifyChanged(object sender, LanguageEventArgs e);
		void NotifyRemoved(object sender, LanguageEventArgs e);
	}
}
