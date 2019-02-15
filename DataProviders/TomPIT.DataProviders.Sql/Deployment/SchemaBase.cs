using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class SchemaBase : ISchema
	{
		public string Name { get; set; }
		public string Schema { get; set; }
	}
}
