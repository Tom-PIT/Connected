using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Data.DataProtection;
public interface IEntityProtectionMiddleware : IMiddleware
{
	Task Invoke();
}
