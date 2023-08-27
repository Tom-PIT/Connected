using System.Collections.Generic;
using System.Threading.Tasks;

namespace TomPIT.Data
{
    public enum ConnectionBehavior
    {
        Shared = 1,
        Isolated = 2
    }
    public interface IDataReader<T> : IDataCommand
    {
        Task<List<T>> Query();
        Task<T> Select();
    }
}
