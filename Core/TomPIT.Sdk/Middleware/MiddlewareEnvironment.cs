using TomPIT.Runtime;

namespace TomPIT.Middleware
{
	internal class MiddlewareEnvironment : IMiddlewareEnvironment
	{
		public bool IsInteractive
		{
			get
			{
				var service = Shell.GetService<IRuntimeService>();

				return service.Mode == EnvironmentMode.Runtime && service.SupportsUI;
			}
		}
	}
}
