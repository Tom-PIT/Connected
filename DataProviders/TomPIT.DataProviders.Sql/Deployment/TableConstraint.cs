using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class TableConstraint : SchemaBase, ITableConstraint
	{
		public string Type { get; set; }
	}
}
