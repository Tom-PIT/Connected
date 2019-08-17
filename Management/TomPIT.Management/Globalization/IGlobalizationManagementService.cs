using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Globalization;

namespace TomPIT.Management.Globalization
{
	internal interface IGlobalizationManagementService
	{
		Guid InsertLanguage(string name, int lcid, LanguageStatus status, string mappings);
		void UpdateLanguage(Guid token, string name, int lcid, LanguageStatus status, string mappings);
		void DeleteLanguage(Guid token);
	}
}