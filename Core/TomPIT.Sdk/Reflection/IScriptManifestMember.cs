namespace TomPIT.Reflection
{
	public interface IScriptManifestMember
	{
		string Documentation { get; set; }
		string Name { get; set; }
		string Type { get; set; }
		string ContainingType { get; set; }
		string BaseType { get; set; }
		string BaseTypeMetaDataName { get; set; }
		string MetaDataName { get; set; }

		IScriptManifestSymbolLocation Location { get; }
	}
}
