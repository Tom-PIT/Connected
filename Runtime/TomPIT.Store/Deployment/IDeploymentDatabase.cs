using System.Collections.Generic;

namespace TomPIT.Deployment
{
	public interface IDeploymentDatabase : IDeploymentEntity
	{
		List<IDeploymentDatabaseTable> Tables { get; }
		List<IDeploymentDatabaseView> Views { get; }
		List<IDeploymentDatabaseProcedure> Procedures { get; }
		List<IDeploymentConstraint> Constraints { get; }
	}
}
