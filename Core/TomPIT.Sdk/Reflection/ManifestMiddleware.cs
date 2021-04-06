namespace TomPIT.Reflection
{
	public abstract class ManifestMiddleware : IManifestMiddleware
	{
		public string Name { get; set; }
		//public string Documentation { get; set; }
		public IManifestType DeclaredType { get; set; }
		public IScriptManifestPointer Address { get; set; }
	}
}
