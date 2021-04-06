namespace TomPIT.Reflection
{
	internal class ScriptManifestSymbolReference : IScriptManifestSymbolReference
	{
		private IScriptManifestSymbolLocation _location;

		public IScriptManifestSymbolLocation Location => _location ??= new ScriptManifestSymbolLocation();

		public string Identifier {get;set;}

		public ScriptManifestSourceReferenceType Type {get;set;}

		public short Address { get; set; }
	}
}
