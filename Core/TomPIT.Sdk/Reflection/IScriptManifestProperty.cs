namespace TomPIT.Reflection
{
	public interface IScriptManifestProperty : IScriptManifestMember, IScriptManifestAttributeMember
	{
		bool CanRead { get; }
		bool CanWrite { get; }
		bool IsPublic { get; }
	}
}