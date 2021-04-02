namespace TomPIT.Reflection
{
	public interface IComponentManifest
	{
		string MicroService { get; }
		string Name { get; }
		string Category { get; }

		void LoadMetaData();
	}
}
