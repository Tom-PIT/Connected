using System.Collections.Generic;

namespace TomPIT.Data
{
	public interface IDataReader<T> : IDataCommand
	{
		List<T> Query();
		T Select();
	}
}
