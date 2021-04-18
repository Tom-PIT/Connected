namespace TomPIT.Reflection
{
	public abstract class ComponentManifestMiddleware : ManifestMiddleware, IComponentManifest
	{
		private bool _metaDataLoaded = false;
		public string MicroService { get; set; }
		public string Category { get; set; }

		protected ComponentManifestMiddleware(IComponentManifestProvider provider)
		{
			Provider = provider;
		}

		private IComponentManifestProvider Provider { get; }

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
