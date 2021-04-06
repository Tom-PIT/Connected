namespace TomPIT.Reflection
{
	public enum ScriptManifestSourceReferenceType
	{
		Other = 0,
		Type = 1,
		Method = 2,
		Field = 3,
		Event = 4,
		Property = 5,
		Local = 6
	}
	public interface IScriptManifestSymbolReference
	{
		short Address { get; }
		string Identifier { get; }
		IScriptManifestSymbolLocation Location { get; }
		ScriptManifestSourceReferenceType Type { get; }
	}
}
