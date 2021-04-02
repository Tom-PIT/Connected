namespace TomPIT.Reflection
{
	internal class ManifestSymbolLocation : IManifestSymbolLocation
	{
		public int StartLine {get;set;}

		public int StartCharacter {get;set;}

		public int EndLine {get;set;}

		public int EndCharacter {get;set;}
	}
}
