using System;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Data
{
    public interface IUniqueValueProvider
    {
        [Obsolete("Please use Async method")]
        bool IsUnique(IMiddlewareContext context, string propertyName);

        Task<bool> IsUniqueAsync(IMiddlewareContext context, string propertyName);
    }
}
