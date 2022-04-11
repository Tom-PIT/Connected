using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TomPIT.Data.Sql;
using TomPIT.Security;
using TomPIT.Serialization;

namespace TomPIT.SysDb.Sql.Security
{
    public class XmlKey : DatabaseRecord, IXmlKey
    {
        public XElement Element { get; set; }
        public string Id { get; set; }
        public string FriendlyName { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            Id = GetString("id");
            FriendlyName = GetString("friendly_name");

            var elementString = GetString("element");

            Element = XElement.Parse(elementString, LoadOptions.PreserveWhitespace);
        }
    }
}
