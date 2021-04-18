namespace TomPIT.ComponentModel.Resources
{
	public interface IScriptBundleInitializer
	{
		IScriptSource CreateDefaultFile(IScriptBundleConfiguration configuration);
	}
}
