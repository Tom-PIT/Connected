namespace TomPIT.Reflection
{
	public interface IManifestField : IManifestMember, IManifestAttributeMember
	{
		bool IsConstant { get; }
	}
}
