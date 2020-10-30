using System.Data;

namespace TomPIT.Data.DataProviders
{
	internal class CommandParameter : ICommandParameter
	{
		public ParameterDirection Direction { get; set; }
		public string Name { get; set; }
		public object Value { get; set; }
		public DbType DataType { get; set; }
	}
}
