using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TomPIT.Data
{
	public interface IDataParameter
	{
		string Name { get; set; }
		object Value { get; set; }
		ParameterDirection Direction { get; set; }

	}
}
