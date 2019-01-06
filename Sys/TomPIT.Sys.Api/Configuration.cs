using Microsoft.Extensions.Configuration;
using System.IO;
using TomPIT.Sys.Api.Environment;

namespace TomPIT.Sys.Api
{
	public static class Configuration
	{
		public static event EnvironmentVariableChangedHandler EnvironmentVariableChanged;

		static Configuration()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("sys.json");

			Root = builder.Build();
		}

		public static IConfigurationRoot Root { get; }

		public static void NotifyEnvironmentVariableChanged(object sender, EnvironmentVariableChangedArgs e)
		{
			EnvironmentVariableChanged?.Invoke(sender, e);
		}

	}
}
