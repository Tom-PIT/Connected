namespace TomPIT.Reflection
{
	public interface IScriptManifestField : IScriptManifestMember, IScriptManifestAttributeMember
	{
		bool IsConstant { get; }
		bool IsPublic { get; }
	}
}
