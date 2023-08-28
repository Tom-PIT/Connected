using System.Data;

namespace TomPIT.Data.Storage;
public interface IStorageParameter
{
    string? Name { get; init; }

    object? Value { get; set; }

    ParameterDirection Direction { get; init; }

    DbType Type { get; init; }
}
