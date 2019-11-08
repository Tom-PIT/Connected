using TomPIT.Collections;

namespace TomPIT.ComponentModel.Resources
{
	public interface IScriptBundleConfiguration : IConfiguration
	{
		ListItems<IScriptSource> Scripts { get; }

		bool Minify { get; }
	}
}
