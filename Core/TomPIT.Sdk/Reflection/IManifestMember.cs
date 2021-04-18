namespace TomPIT.Reflection
{
	public interface IManifestMember
	{
		string Documentation { get; set; }
		string Name { get; set; }
		string Type { get; set; }
	}
}