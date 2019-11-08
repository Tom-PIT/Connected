namespace TomPIT.Reflection.Manifests.Entities
{
	public abstract class ComponentManifest : IComponentManifest
	{
		public string MicroService { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
	}
}
