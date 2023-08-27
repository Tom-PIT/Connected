using System.Collections.Immutable;
using System.Data;

namespace TomPIT.Data.Storage;
public interface IStorageOperation
{
    string? CommandText { get; }

    CommandType CommandType { get; }

    ImmutableList<IStorageParameter>? Parameters { get; }

    int CommandTimeout { get; }

    DataConcurrencyMode Concurrency { get; }
}
