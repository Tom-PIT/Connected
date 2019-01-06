using System.Collections.Generic;
using System.Linq;
using TomPIT.Configuration;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.SysDb.Management;

namespace TomPIT.SysDb.Sql.Configuration
{
	internal class SettingHandler : ISettingHandler
	{
		public void Delete(IResourceGroup resourceGroup, string name)
		{
			var d = Select(resourceGroup, name);

			if (d == null)
				return;

			var p = new Writer("tompit.setting_del");

			p.CreateParameter("@resource_group", resourceGroup.GetId());
			p.CreateParameter("@name", name);

			p.Execute();
		}

		public List<ISetting> Query()
		{
			return new Reader<Setting>("tompit.setting_sel").Execute().ToList<ISetting>();
		}

		public ISetting Select(IResourceGroup resourceGroup, string name)
		{
			var p = new Reader<Setting>("tompit.setting_sel");

			p.CreateParameter("@resource_group", resourceGroup.GetId());
			p.CreateParameter("@name", name);

			return p.ExecuteSingleRow();
		}

		public void Update(IResourceGroup resourceGroup, string name, string value, bool visible, DataType dataType, string tags)
		{
			var w = new Writer("tompit.setting_mdf");

			w.CreateParameter("@resource_group", resourceGroup.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@visible", visible);
			w.CreateParameter("@data_type", dataType);
			w.CreateParameter("@tags", tags, true);
			w.CreateParameter("@value", value, true);

			w.Execute();
		}
	}
}
