using System.Data.SqlClient;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class ViewSynchronizer : SynchronizerBase
	{
		public ViewSynchronizer(SqlCommand command, IModelSchema schema) : base(command, schema)
		{

		}
	}
}
