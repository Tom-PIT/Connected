namespace TomPIT.Reflection
{
	public interface IManifestProperty : IManifestMember, IManifestAttributeMember
	{
		bool CanRead { get; }
		bool CanWrite { get; }
	}
}