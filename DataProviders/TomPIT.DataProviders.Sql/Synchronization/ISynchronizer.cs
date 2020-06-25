using System.Data.SqlClient;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	public enum ConstraintNameType
	{
		Index = 1,
		PrimaryKey = 2
	}
	internal interface ISynchronizer
	{
		void Execute();
		SqlCommand CreateCommand();
		SqlCommand CreateCommand(string commandText);

		IModelSchema Model { get; }

		public string GenerateConstraintName(ConstraintNameType type);
	}
}
