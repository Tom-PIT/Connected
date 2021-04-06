namespace TomPIT.Reflection
{
	public interface IScriptManifestProvider
	{
		IScriptManifestType CreateTypeInstance(ITypeSymbolDescriptor descriptor);

		void ProcessManifestType(ITypeSymbolDescriptor descriptor, IScriptManifestType type);
	}
}
