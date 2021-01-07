/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace TomPIT.Connected.Printing.Client.Configuration
{
    public class MappingsConfigurationHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var deviceConfigList = new Dictionary<string, string>();

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.Name.Equals("mapping", StringComparison.OrdinalIgnoreCase))
                {
                    var resourceName = GetValue<string>(node, "resourceName");
                    var printerName = GetValue<string>(node, "printerName");

                    if (string.IsNullOrWhiteSpace(resourceName) || string.IsNullOrWhiteSpace(printerName))
                        continue;

                    deviceConfigList[resourceName] = printerName;
                }
            }

            return deviceConfigList;
        }

        private T GetValue<T>(XmlNode node, string attributeName)
        {
            if (node.Attributes[attributeName] is null)
                throw new Exception(string.Format($"Attribute expected. ({attributeName})"));

            XmlAttribute att = node.Attributes[attributeName];

            string result = att?.Value ?? string.Empty;

            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}
