using System.Collections.Immutable;

namespace TomPIT.Data.Schema;
public interface IExistingSchemaColumn
{
	ImmutableArray<string> QueryIndexColumns(string column);
}
