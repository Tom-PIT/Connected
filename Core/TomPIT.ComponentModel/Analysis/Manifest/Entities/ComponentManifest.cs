using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Analysis.Manifest.Entities
{
	public abstract class ComponentManifest : IComponentManifest
	{
		public string MicroService { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
	}
}
