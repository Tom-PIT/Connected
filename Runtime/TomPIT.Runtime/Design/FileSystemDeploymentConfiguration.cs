using Microsoft.Extensions.Configuration;

using System.Text.Json;

namespace TomPIT.Design
{
	internal class FileSystemDeploymentConfiguration : IFileSystemDeploymentConfiguration
	{
		private readonly static ConfigurationBindings _binder = new();

		public FileSystemDeploymentConfiguration()
		{
			Initialize();
		}

		public bool Enabled => _binder.Enabled;

		public string Path => _binder.Path ?? string.Empty;

		private void Initialize()
		{
			Shell.Configuration.Bind("deployment:fileSystem", _binder);
		}

		private class ConfigurationBindings
		{
			public bool Enabled { get; set; }

			public string? Path { get; set; }
		}
	}
}
