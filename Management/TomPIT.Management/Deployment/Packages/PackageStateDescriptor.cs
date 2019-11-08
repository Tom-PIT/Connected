using System;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Deployment.Packages
{
	internal class PublishedPackage : IPackageStateDescriptor
	{
		public string Name { get; set; }
		public string Title { get; set; }
		public int Major { get; set; }
		public int Minor { get; set; }
		public int Build { get; set; }
		public int Revision { get; set; }
		public int ShellMajor { get; set; }
		public int ShellMinor { get; set; }
		public int ShellBuild { get; set; }
		public int ShellRevision { get; set; }
		public DateTime Created { get; set; }
		public string Description { get; set; }
		public string ProjectUrl { get; set; }
		public string ImageUrl { get; set; }
		public string LicenseUrl { get; set; }
		public string Licenses { get; set; }
		public string Url { get; set; }
		public string Company { get; set; }
		public string Website { get; set; }
		public Guid Token { get; set; }
		public Guid Service { get; set; }
		public Guid Plan { get; set; }
		public bool Author { get; set; }
		public int DependencyCount { get; set; }
		public PackageState State { get; set; }
	}
}