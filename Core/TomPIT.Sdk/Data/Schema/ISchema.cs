using System;
using System.Collections.Generic;

namespace TomPIT.Data.Schema;
public interface ISchema : IEquatable<ISchema>
{
	List<ISchemaColumn> Columns { get; }

	string? Schema { get; }
	string? Name { get; }
	string? Type { get; }
	bool Ignore { get; }
}
