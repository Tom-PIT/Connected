namespace TomPIT.Reflection
{
	public interface IManifestMiddleware
	{
		string Name { get; }
		//string Documentation { get; }
		IManifestType DeclaredType { get; }
		IScriptManifestPointer Address { get; }
	}
}