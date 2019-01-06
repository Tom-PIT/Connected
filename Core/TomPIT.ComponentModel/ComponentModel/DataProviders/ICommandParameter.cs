using System;
using System.Data;

namespace TomPIT.ComponentModel.DataProviders
{
	public interface ICommandParameter
	{
		string Name { get; }
		Type DataType { get; }
		object Value { get; set; }
		ParameterDirection Direction { get; }
	}
}
