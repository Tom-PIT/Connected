using System;
using System.Data;
using TomPIT.ComponentModel.DataProviders;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class CommandParameter : ICommandParameter
	{
		public Type DataType { get; set; }
		public ParameterDirection Direction { get; set; }
		public string Name { get; set; }
		public object Value { get; set; }
	}
}
