using System.Collections.Generic;

namespace TomPIT.Data
{
	public interface IModelSchema
	{
		List<IModelSchemaColumn> Columns { get; }

		string Schema { get; }
		string Name { get; }
	}
}
