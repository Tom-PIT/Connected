using Newtonsoft.Json;
using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PublishedPackage : IPublishedPackage
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
		[JsonProperty("major")]
		public int Major { get; set; }
		[JsonProperty("minor")]
		public int Minor { get; set; }
		[JsonProperty("build")]
		public int Build { get; set; }
		[JsonProperty("revision")]
		public int Revision { get; set; }
		[JsonProperty("shellMajor")]
		public int ShellMajor { get; set; }
		[JsonProperty("shellMinor")]
		public int ShellMinor { get; set; }
		[JsonProperty("shellBuild")]
		public int ShellBuild { get; set; }
		[JsonProperty("shellRevision")]
		public int ShellRevision { get; set; }
		[JsonProperty("scope")]
		public PackageScope Scope { get; set; }
		[JsonProperty("created")]
		public DateTime Created { get; set; }
		[JsonProperty("price")]
		public double Price { get; set; }
		[JsonProperty("description")]
		public string Description { get; set; }
		[JsonProperty("projectUrl")]
		public string ProjectUrl { get; set; }
		[JsonProperty("imageUrl")]
		public string ImageUrl { get; set; }
		[JsonProperty("licenseUrl")]
		public string LicenseUrl { get; set; }
		[JsonProperty("trial")]
		public bool Trial { get; set; }
		[JsonProperty("trialPeriod")]
		public int TrialPeriod { get; set; }
		[JsonProperty("licenses")]
		public string Licenses { get; set; }
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("company")]
		public string Company { get; set; }
		[JsonProperty("website")]
		public string Website { get; set; }
		[JsonProperty("token")]
		public Guid Token { get; set; }
	}
}
