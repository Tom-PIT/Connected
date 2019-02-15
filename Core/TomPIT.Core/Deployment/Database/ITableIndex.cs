using System.Collections.Generic;

namespace TomPIT.Deployment.Database
{
	public interface ITableIndex
	{
		string Name { get; }
		List<string> Columns { get; }
	}
}
