namespace TomPIT.ComponentModel.Scripting
{
	public interface IScriptConfiguration : IConfiguration, IText
	{
		ElementScope Scope { get; }
	}
}
