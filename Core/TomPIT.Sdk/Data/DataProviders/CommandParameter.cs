using System;
using System.Data;

namespace TomPIT.Data.DataProviders
{
	internal class CommandParameter : ICommandParameter
	{
		public Type DataType { get; set; }
		public ParameterDirection Direction { get; set; }
		public string Name { get; set; }
		public object Value { get; set; }
	}
}
