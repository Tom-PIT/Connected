using System.Collections.Generic;

namespace TomPIT.Deployment
{
	public interface IDeploymentDatabaseTable : IDeploymentDatabaseEntity
	{
		string Schema { get; }
		List<IDeploymentTableColumn> Columns { get; }
	}
}
