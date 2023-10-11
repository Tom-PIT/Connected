namespace TomPIT.ComponentModel.UI
{
	public interface IViewConfiguration : IConfiguration, IGraphicInterface
	{
		//IServerEvent Invoke { get; }

		string Url { get; }
		string Layout { get; }
		bool Enabled { get; }
		bool AuthorizationEnabled { get; }
	}
}
