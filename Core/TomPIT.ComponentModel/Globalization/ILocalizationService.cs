using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Globalization
{
	public interface ILocalizationService
	{
		string GetString(string microService, string stringTable, string key, int lcid, bool throwException);
	}
}
