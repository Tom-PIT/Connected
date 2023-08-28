using System.Threading.Tasks;

namespace TomPIT.Data.Storage;
public interface ITransactionClient
{
    Task Commit();

    Task Rollback();
}
