using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Features
{
	public interface IFeature : IConfigurationElement
	{
		string Name { get; }
		bool Enabled { get; }
	}
}
