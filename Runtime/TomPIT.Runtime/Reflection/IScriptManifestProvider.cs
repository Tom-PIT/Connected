namespace TomPIT.Reflection
{
	public interface IScriptManifestProvider
	{
		IManifestType CreateTypeInstance(ITypeSymbolDescriptor descriptor);

		void ProcessManifestType(ITypeSymbolDescriptor descriptor, IManifestType type);
	}
}
