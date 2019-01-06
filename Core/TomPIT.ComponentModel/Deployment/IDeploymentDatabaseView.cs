namespace TomPIT.Deployment
{
	public interface IDeploymentDatabaseView : IDeploymentDatabaseEntity
	{
		string Schema { get; }
		string Text { get; }
	}
}
