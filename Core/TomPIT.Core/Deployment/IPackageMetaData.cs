using System;

namespace TomPIT.Deployment
{
	public interface IPackageMetaData
	{
		Guid Service { get; }
		Guid Plan { get; }
		string Name { get; }
		string Title { get; }
		string Version { get; }
		Guid Account { get; }
		DateTime Created { get; }
		string ShellVersion { get; }
		string Description { get; }
		string ProjectUrl { get; }
		string LicenseUrl { get; }
		string Tags { get; }
		string ImageUrl { get; }
		string Licenses { get; }
	}
}
