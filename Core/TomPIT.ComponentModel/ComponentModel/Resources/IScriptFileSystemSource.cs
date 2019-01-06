namespace TomPIT.ComponentModel.Resources
{
	public interface IScriptFileSystemSource : IScriptSource
	{
		string VirtualPath { get; }
	}
}
