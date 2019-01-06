namespace TomPIT.Deployment
{
	public interface IDeploymentDatabaseProcedure : IDeploymentDatabaseEntity
	{
		string Schema { get; }
		string Text { get; }
	}
}
