using System;
using System.ComponentModel;

namespace TomPIT.MicroServices.Reporting.Storage
{
	internal class DataSourceSite : ISite
	{
		public DataSourceSite(string connection)
		{
			Connection = connection;
		}
		public IComponent Component { get; set; }

		public IContainer Container { get; set; }

		public bool DesignMode { get; set; }

		public string Name { get; set; }

		public string Connection { get; set; }

		public object GetService(Type serviceType)
		{
			return null;
		}
	}
}
