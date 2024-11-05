using System.Threading.Tasks;

namespace TomPIT.Middleware;
public interface IMultiContextOrchestrator
{
	Task Commit();
	Task Rollback();
}
