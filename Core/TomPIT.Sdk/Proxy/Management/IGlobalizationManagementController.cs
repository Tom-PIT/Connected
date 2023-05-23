using System;
using TomPIT.Globalization;

namespace TomPIT.Proxy.Management;

public interface IGlobalizationManagementController
{
    Guid InsertLanguage(string name, int lcid, LanguageStatus status, string mappings);
    void UpdateLanguage(Guid token, string name, int lcid, LanguageStatus status, string mappings);
    void DeleteLanguage(Guid token);
}
