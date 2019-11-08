
using TomPIT.Deployment.Database;

namespace TomPIT.Deployment
{
	public interface IDatabaseDeploymentContext
	{
		string ConnectionString { get; }
		T GetService<T>();
		IDatabase Database { get; }

		IDatabase LoadState(IDatabaseDeploymentContext context);
		void SaveState();
	}
}
