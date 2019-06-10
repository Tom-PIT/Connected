using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Features
{
	public interface IFeatureSet : IConfiguration
	{
		ListItems<IFeature> Features { get; }
	}
}
