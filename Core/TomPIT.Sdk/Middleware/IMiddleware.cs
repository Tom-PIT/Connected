using System.Threading.Tasks;

namespace TomPIT.Middleware;
public interface IMiddleware
{
    Task Initialize();
}
