using System;
using System.Collections.Generic;

namespace TomPIT.Data
{
	public interface IModelSchema : IEquatable<IModelSchema>
	{
		List<IModelSchemaColumn> Columns { get; }

		string Schema { get; }
		string Name { get; }
		string Type { get; }
		bool Ignore { get; }
		string Dependency { get; }
	}
}
