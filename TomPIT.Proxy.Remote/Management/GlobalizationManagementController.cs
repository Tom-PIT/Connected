using System;
using TomPIT.Globalization;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class GlobalizationManagementController : IGlobalizationManagementController
{
    private const string Controller = "GlobalizationManagement";
    public void DeleteLanguage(Guid token)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteLanguage"), new
        {
            token
        });
    }

    public Guid InsertLanguage(string name, int lcid, LanguageStatus status, string mappings)
    {
        return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertLanguage"), new
        {
            name,
            lcid,
            status,
            mappings
        });
    }

    public void UpdateLanguage(Guid token, string name, int lcid, LanguageStatus status, string mappings)
    {
        Connection.Post(Connection.CreateUrl(Controller, "UpdateLanguage"), new
        {
            name,
            lcid,
            status,
            mappings,
            token
        });
    }
}
