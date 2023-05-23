using System;
using TomPIT.Globalization;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class GlobalizationManagementController : IGlobalizationManagementController
{
    public void DeleteLanguage(Guid token)
    {
        DataModel.Languages.Delete(token);
    }

    public Guid InsertLanguage(string name, int lcid, LanguageStatus status, string mappings)
    {
        return DataModel.Languages.Insert(name, lcid, status, mappings);
    }

    public void UpdateLanguage(Guid token, string name, int lcid, LanguageStatus status, string mappings)
    {
        DataModel.Languages.Update(token, name, lcid, status, mappings);
    }
}
