namespace TomPIT.Deployment.Database
{
	public interface IRoutine : ISchema
	{
		string Type { get; }
		string Definition { get; }
	}
}
