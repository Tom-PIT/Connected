/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Sdk.HealthMonitoring;

namespace TomPIT.Sdk.IoT.Devices
{
    public abstract partial class BaseIotDevice : IEndPointHealthMonitoring
    {
        protected Dictionary<string, string> ConnectionSettings { get; private set; }

        private void ParseConnectionString(string connectionString)
        {
            ConnectionSettings = connectionString.Split(';')
                .Select(value => value.Split('='))
                .ToDictionary(keyValuePair => AddDefaultNameSpace(keyValuePair[0].Trim().ToLower()), keyValuePair => keyValuePair[1].Trim());
        }

        private static string AddDefaultNameSpace(string name)
        {
            return name.Contains(".")
                ? name
                : $"default.{name}";
        }

        protected T GetConnectionSetting<T>(string name, T defaultValue)
        {
            return GetConnectionSetting<T>("default", name, defaultValue);
        }

        protected T GetConnectionSetting<T>(string nameSpace, string name, T defaultValue)
        {
            var keyName = $"{nameSpace}.{name}";

            if (!ConnectionSettings.ContainsKey(keyName.ToLower()))
                return defaultValue;

            var value = ConnectionSettings[keyName.ToLower()];

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
