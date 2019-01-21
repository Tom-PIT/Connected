namespace TomPIT.Data.DataProviders.Deployment
{
	public interface ITableConstraint : ISchema
	{
		string Type { get; }
	}
}
