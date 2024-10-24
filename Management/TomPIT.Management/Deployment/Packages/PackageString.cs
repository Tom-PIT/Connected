﻿using System;
using Newtonsoft.Json;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment.Packages
{
	internal class PackageString : IPackageString
	{
		[JsonProperty(PropertyName = "element")]
		public Guid Element { get; set; }
		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }
		[JsonProperty(PropertyName = "lcid")]
		public int Lcid { get; set; }
		[JsonProperty(PropertyName = "property")]
		public string Property { get; set; }
	}
}
