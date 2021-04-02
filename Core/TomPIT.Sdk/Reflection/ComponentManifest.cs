namespace TomPIT.Reflection
{
	public abstract class ComponentManifest : IComponentManifest
	{
		private bool _metaDataLoaded = false;
		public string MicroService { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }

		protected ComponentManifest(IComponentManifestProvider provider)
		{
			Provider = provider;
		}

		protected IComponentManifestProvider Provider { get; }

		public void LoadMetaData()
		{
			if (_metaDataLoaded)
				return;

			_metaDataLoaded = true;

			OnLoadMetaData();
		}

		protected virtual void OnLoadMetaData()
		{
			Provider.LoadMetaData(this);
		}
	}
}
