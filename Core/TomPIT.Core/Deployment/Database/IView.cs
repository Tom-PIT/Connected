namespace TomPIT.Deployment.Database
{
	public interface IView : ISchema
	{
		string Definition { get; }
	}
}
