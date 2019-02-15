using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class TableConstraint : SchemaBase, ITableConstraint
	{
		public string Type { get; set; }
	}
}
