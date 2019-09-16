namespace TomPIT.ComponentModel.Scripting
{
	public interface IScriptConfiguration : IConfiguration, ISourceCode
	{
		ElementScope Scope { get; }
	}
}
