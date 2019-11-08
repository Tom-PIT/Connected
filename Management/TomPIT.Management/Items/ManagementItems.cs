namespace TomPIT.Management.Items
{
	internal static class ManagementItems
	{
		private const string ItemsNamespace = "TomPIT.Management.Items";

		public const string StorageProvider = ItemsNamespace + ".StorageProviderItems, " + SystemAssemblies.ManagementAssembly;
		public const string MicroServiceTemplates = ItemsNamespace + ".MicroServiceTemplatesItems, " + SystemAssemblies.ManagementAssembly;
	}
}
