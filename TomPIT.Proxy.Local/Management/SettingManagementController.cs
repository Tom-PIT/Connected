﻿using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management
{
    internal class SettingManagementController : ISettingManagementController
    {
        public void Delete(string name, string nameSpace, string type, string primaryKey)
        {
            DataModel.Settings.Delete(name, nameSpace, type, primaryKey);
        }

        public void Update(string name, string nameSpace, string type, string primaryKey, object value)
        {
            DataModel.Settings.Update(name, nameSpace, type, primaryKey, Types.Convert<string>(value));
        }
    }
}
