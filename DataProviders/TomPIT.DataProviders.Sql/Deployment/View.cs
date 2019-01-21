using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class View : SchemaBase, IView
	{
		public string Definition { get; set; }
	}
}
