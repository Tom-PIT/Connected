using System;

namespace TomPIT.Deployment
{
	public enum PackageScope
	{
		Public = 1,
		Private = 2
	}

	public interface IPackageMetaData
	{
		Guid Id { get; }
		string Name { get; }
		string Title { get; }
		string Version { get; }
		PackageScope Scope { get; }
		string Publisher { get; }
		DateTime Created { get; }
		double Price { get; }
		string ShellVersion { get; }
		string Description { get; }
		string ProjectUrl { get; }
		string LicenseUrl { get; }
		string Tags { get; }
		string ImageUrl { get; }
		bool Trial { get; }
		int TrialPeriod { get; }
		string Licenses { get; }
	}
}
