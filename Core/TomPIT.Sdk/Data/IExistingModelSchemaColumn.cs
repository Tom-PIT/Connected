using System.Collections.Generic;

namespace TomPIT.Data
{
	public interface IExistingModelSchemaColumn
	{
		List<string> QueryIndexColumns(string column);
	}
}
