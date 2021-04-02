namespace TomPIT.Reflection
{
	public interface IManifestSymbolLocation
	{
		int StartLine { get; set; }
		int StartCharacter { get; set; }

		int EndLine { get; set; }
		int EndCharacter { get; set; }
	}
}
