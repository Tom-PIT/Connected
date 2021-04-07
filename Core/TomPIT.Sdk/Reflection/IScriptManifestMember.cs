namespace TomPIT.Reflection
{
	public interface IScriptManifestMember
	{
		string Documentation { get; set; }
		string Name { get; set; }
		string Type { get; set; }
		string ContainingType { get; set; }

		IScriptManifestSymbolLocation Location { get; }
	}
}
