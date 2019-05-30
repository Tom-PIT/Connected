using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel
{
	public interface IManifest
	{
		string MicroService { get; }
		string Name { get; }
	}
}
