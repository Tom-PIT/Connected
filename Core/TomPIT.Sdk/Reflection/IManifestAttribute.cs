namespace TomPIT.Reflection
{
	public interface IManifestAttribute : IManifestMember
	{
		bool IsValidation { get; }
		string Description { get; }
	}
}
