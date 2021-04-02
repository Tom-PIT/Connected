namespace TomPIT.Reflection
{
	public enum ManifestSourceReferenceType
	{
		Other = 0,
		Type = 1,
		Method = 2,
		Field = 3,
		Event = 4,
		Property = 5,
		Local = 6
	}
	public interface IManifestSymbolReference
	{
		short Address { get; }
		string Identifier { get; }
		IManifestSymbolLocation Location { get; }
		ManifestSourceReferenceType Type { get; }
	}
}
