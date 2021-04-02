namespace TomPIT.Reflection
{
	internal class ManifestSymbolReference : IManifestSymbolReference
	{
		private IManifestSymbolLocation _location;

		public IManifestSymbolLocation Location => _location ??= new ManifestSymbolLocation();

		public string Identifier {get;set;}

		public ManifestSourceReferenceType Type {get;set;}

		public short Address { get; set; }
	}
}
