using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TomPIT.Data.Sql;
using TomPIT.Security;
using TomPIT.Serialization;
using TomPIT.SysDb.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class XmlKeyHandler : IXmlKeyHandler
	{
		public void Upsert(string id, string friendlyName, string encryptedElement)
		{
			using var w = new Writer("tompit.xml_key_ups");

			w.CreateParameter("@element", encryptedElement);
			w.CreateParameter("@friendly_name", friendlyName);
			w.CreateParameter("@id", id);

			w.Execute();
		}

		public List<IXmlKey> Query()
		{
			using var r = new Reader<XmlKey>("tompit.xml_key_que");

			return r.Execute().ToList<IXmlKey>();
		}

        public IXmlKey Select(string id)
        {
			using var r = new Reader<XmlKey>("tompit.xml_key_sel");

			r.CreateParameter("@id", id);

			return r.ExecuteSingleRow();
		}
    }
}
