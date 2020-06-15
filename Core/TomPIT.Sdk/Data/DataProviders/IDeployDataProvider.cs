using System.Collections.Generic;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;

namespace TomPIT.Data.DataProviders
{
	public interface IDeployDataProvider
	{
		IDatabase CreateSchema(string connectionString);
		void Deploy(IDatabaseDeploymentContext context);
		void CreateDatabase(string connectionString);

		void Synchronize(string connectionString, IModelSchema schema, List<IModelOperationSchema> procedures);
	}
}
