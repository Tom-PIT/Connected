using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Validation;
/// <summary>
/// Represents Validation middleware. Implementations should define <see cref="TargetAttribute"/> attribute
/// to tell the middleware which component and (optional) method to validate.
/// </summary>
public interface IValidator : IMiddleware
{
    Task Validate();
}
