using TomPIT.Collections;

namespace TomPIT.ComponentModel.Features
{
	public interface IFeatureSetConfiguration : IConfiguration
	{
		ListItems<IFeature> Features { get; }
	}
}
