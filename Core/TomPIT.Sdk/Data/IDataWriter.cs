using System.Threading.Tasks;

namespace TomPIT.Data
{
    public interface IDataWriter : IDataCommand
    {
        Task<int> Execute();
        Task<T> Execute<T>();

        IDataParameter SetReturnValueParameter(string name);
    }
}
