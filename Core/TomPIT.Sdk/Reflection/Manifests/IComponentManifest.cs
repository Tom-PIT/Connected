using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Reflection.Manifests
{
	public interface IComponentManifest
	{
		string MicroService { get; }
		string Name { get; }
		string Category { get; }
	}
}
