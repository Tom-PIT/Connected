using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	public interface ISettingsMiddleware : IMiddlewareComponent
	{
		string Type { get; set; }
		string PrimaryKey { get; set; }
	}
}
