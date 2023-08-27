using System;
using System.Threading.Tasks;
using TomPIT.Data;

namespace TomPIT.Middleware
{
    public interface IMiddlewareComponent : IUniqueValueProvider, IMiddlewareObject
    {
        [Obsolete("Please use Async method")]
        void Validate();
        Task ValidateAsync();
    }
}
