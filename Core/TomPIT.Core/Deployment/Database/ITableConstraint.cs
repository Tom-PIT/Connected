namespace TomPIT.Deployment.Database
{
	public interface ITableConstraint : ISchema
	{
		string Type { get; }
	}
}
