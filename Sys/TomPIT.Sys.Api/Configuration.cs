using TomPIT.Sys.Api.Environment;

namespace TomPIT.Sys.Api
{
	public static class Configuration
	{
		public static event EnvironmentVariableChangedHandler EnvironmentVariableChanged;

		public static void NotifyEnvironmentVariableChanged(object sender, EnvironmentVariableChangedArgs e)
		{
			EnvironmentVariableChanged?.Invoke(sender, e);
		}
	}
}
