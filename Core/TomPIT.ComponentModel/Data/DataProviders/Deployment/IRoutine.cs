namespace TomPIT.Data.DataProviders.Deployment
{
	public interface IRoutine : ISchema
	{
		string Type { get; }
		string Definition { get; }
	}
}
