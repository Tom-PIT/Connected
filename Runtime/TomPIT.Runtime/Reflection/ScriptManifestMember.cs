namespace TomPIT.Reflection
{
	internal class ScriptManifestMember : IScriptManifestMember
	{
		private IScriptManifestSymbolLocation _location;
		public string Documentation { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }

		public IScriptManifestSymbolLocation Location => _location ??= new ScriptManifestSymbolLocation();

		public string ContainingType { get; set; }
		public string BaseType { get; set; }
		public string BaseTypeMetaDataName { get; set; }
		public string MetaDataName { get; set; }
	}
}
