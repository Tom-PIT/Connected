namespace TomPIT.ComponentModel.Resources
{
	public interface IScriptBundle
	{
		ListItems<IScriptSource> Scripts { get; }

		bool Minify { get; }
	}
}
