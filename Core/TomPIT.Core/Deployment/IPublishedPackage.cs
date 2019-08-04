using System;

namespace TomPIT.Deployment
{
	public interface IPublishedPackage
	{
		string Name { get; }
		string Title { get; }
		int Major { get; }
		int Minor { get; }
		int Build { get; }
		int Revision { get; }
		int ShellMajor { get; }
		int ShellMinor { get; }
		int ShellBuild { get; }
		int ShellRevision { get; }
		DateTime Created { get; }
		string Description { get; }
		string ProjectUrl { get; }
		string ImageUrl { get; }
		string LicenseUrl { get; }
		string Licenses { get; }
		string Url { get; }
		string Company { get; }
		string Website { get; }
		Guid Token { get; }
		Guid Service { get; }
		Guid Plan { get; }
		bool Author { get; }
		int DependencyCount { get; }
	}
}