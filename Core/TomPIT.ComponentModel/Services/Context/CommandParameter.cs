using System;
using System.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.Services.Context
{
	internal class CommandParameter : ICommandParameter
	{
		public Type DataType { get; set; }
		public ParameterDirection Direction { get; set; }
		public string Name { get; set; }
		public object Value { get; set; }
	}
}
