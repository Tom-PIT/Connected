namespace TomPIT.Reflection
{
	internal class ScriptManifestSymbolReference : IScriptManifestSymbolReference
	{
		public string Identifier {get;set;}

		public ScriptManifestSourceReferenceType Type {get;set;}

		public short Address { get; set; }
	}
}
