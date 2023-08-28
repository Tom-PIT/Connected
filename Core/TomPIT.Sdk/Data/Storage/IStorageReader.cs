using System.Collections.Immutable;
using System.Data;
using System.Threading.Tasks;

namespace TomPIT.Data.Storage;
public interface IStorageReader<T> : IStorageCommand
{
    Task<ImmutableList<T>> Query();
    Task<T?> Select();
    Task<IDataReader> OpenReader();
}
