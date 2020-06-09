using TomPIT.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.Middleware
{
	internal class DataConnectionDescriptor
	{
		public IDataConnection Connection { get; set; }
		public IDataProvider DataProvider { get; set; }
		public string ConnectionString { get; set; }
		public int Id { get; set; }
		public string Arguments { get; set; }
	}
}
