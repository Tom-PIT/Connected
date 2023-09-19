using System.Text.Json;

namespace TomPIT.Design
{
	internal class FileSystemDeploymentConfiguration : IFileSystemDeploymentConfiguration
	{
		public FileSystemDeploymentConfiguration()
		{
			Initialize();
		}

		public bool Enabled { get; private set; }

		public string Path { get; private set; }

		private void Initialize()
		{
			if (!Shell.Configuration.RootElement.TryGetProperty("deployment", out JsonElement element))
				return;

			if (!element.TryGetProperty("fileSystem", out JsonElement fileSystemElement))
				return;

			if (fileSystemElement.TryGetProperty("enabled", out JsonElement enabledElement))
				Enabled = enabledElement.GetBoolean();

			if (fileSystemElement.TryGetProperty("path", out JsonElement pathElement))
				Path = pathElement.GetString();
		}
	}
}
