using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TomPIT.Data
{
	public interface IDataReader<T>:IDataCommand
	{
		List<T> Query();
		T Select();
	}
}
