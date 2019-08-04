using Newtonsoft.Json;
using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageMetaData : IPackageMetaData
	{
		public Guid Service { get; set; }
		public string Name { get; set; }
		public string Title { get; set; }
		public string Version { get; set; }
		public Guid Account { get; set; }
		public DateTime Created { get; set; }
		public string ShellVersion { get; set; }
		public string Description { get; set; }
		public string ProjectUrl { get; set; }
		public string LicenseUrl { get; set; }
		public string Tags { get; set; }
		public string ImageUrl { get; set; }
		public string Licenses { get; set; }
		public Guid Plan { get; set; }
	}
}
