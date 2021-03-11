using Microsoft.Data.SqlClient;

namespace TomPIT.Data.Sql
{
	public abstract class ConnectionBase
	{
		protected abstract string ConnectionKey { get; }

		public abstract SqlConnection Connection { get; }

		public static string DefaultConnectionString { get; set; }

		protected virtual string ConnectionString { get { return DefaultConnectionString; } }
	}
}