using System.Threading.Tasks;

namespace TomPIT.Data.Storage;
public interface IStorageWriter : IStorageCommand
{
    Task<int> Execute();
}
