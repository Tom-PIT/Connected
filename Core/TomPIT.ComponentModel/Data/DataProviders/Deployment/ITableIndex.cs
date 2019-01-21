using System.Collections.Generic;

namespace TomPIT.Data.DataProviders.Deployment
{
	public interface ITableIndex
	{
		string Name { get; }
		List<string> Columns { get; }
	}
}
