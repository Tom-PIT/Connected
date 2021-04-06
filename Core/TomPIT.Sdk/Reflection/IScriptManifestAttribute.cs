namespace TomPIT.Reflection
{
	public interface IScriptManifestAttribute : IScriptManifestMember
	{
		bool IsValidation { get; }
		string Description { get; }
	}
}
