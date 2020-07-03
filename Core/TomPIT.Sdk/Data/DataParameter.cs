using System.Data;

namespace TomPIT.Data
{
	internal class DataParameter : TomPIT.Data.IDataParameter
	{
		public string Name { get; set; }
		public object Value { get; set; }
		public ParameterDirection Direction { get; set; }
		public DbType Type { get; set; }
	}
}
