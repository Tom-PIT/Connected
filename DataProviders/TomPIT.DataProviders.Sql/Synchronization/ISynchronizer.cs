using System.Data.SqlClient;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	public enum ConstraintNameType
	{
		Index = 1,
		PrimaryKey = 2,
		Default = 3
	}
	internal interface ISynchronizer
	{
		void Execute();
		SqlCommand CreateCommand();
		SqlCommand CreateCommand(string commandText);

		IModelSchema Model { get; }

		ExistingModel ExistingModel { get; set; }

		public string GenerateConstraintName(ConstraintNameType type);
	}
}
