using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace TomPIT.Data.Storage;
public interface IAsyncQueryProvider : IQueryProvider
{
    object Execute(Expression expression, CancellationToken cancellationToken = default);
}

