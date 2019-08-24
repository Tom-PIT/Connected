using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Analysis.Manifest
{
	public interface IComponentManifest
	{
		string MicroService { get; }
		string Name { get; }
		string Category { get; }
	}
}
