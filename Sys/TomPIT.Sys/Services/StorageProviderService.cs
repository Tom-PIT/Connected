using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Api.Storage;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Services
{
	internal class StorageProviderService : IStorageProviderService
	{
		public const string DefaultStorageProviderVar = "Default storage provider";

		private Lazy<ConcurrentDictionary<Guid, IStorageProvider>> _providers = new Lazy<ConcurrentDictionary<Guid, IStorageProvider>>();
		private IStorageProvider DefaultStorageProvider { get; set; }

		private ConcurrentDictionary<Guid, IStorageProvider> Providers { get { return _providers.Value; } }

		public StorageProviderService()
		{
			Api.Configuration.EnvironmentVariableChanged += OnEnvironmentVariableChanged;
		}

		private void OnEnvironmentVariableChanged(object sender, TomPIT.Sys.Api.Environment.EnvironmentVariableChangedArgs e)
		{
			if (string.Compare(e.Variable, DefaultStorageProviderVar, true) == 0)
				DefaultStorageProvider = null;
		}

		public void Register(IStorageProvider provider)
		{
			Providers.TryAdd(provider.Token, provider);
		}

		public IStorageProvider Resolve(Guid resourceGroup)
		{
			if (resourceGroup == Guid.Empty)
				return ResolveDefaultStorageProvider();

			var r = DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			if (r.StorageProvider == Guid.Empty)
				return ResolveDefaultStorageProvider();

			var sp = Select(r.StorageProvider);

			if (sp != null)
				return sp;

			throw new SysException(string.Format("{0} ({1})", SR.ErrStorageProviderNotRegistered, r.StorageProvider.ToString()));
		}

		public IStorageProvider Select(Guid token)
		{
			if (Providers.TryGetValue(token, out IStorageProvider r))
				return r;

			return null;
		}

		private IStorageProvider ResolveDefaultStorageProvider()
		{
			if (DefaultStorageProvider != null)
				return DefaultStorageProvider;

			var ev = DataModel.EnvironmentVariables.Select(DefaultStorageProviderVar);

			if (ev == null || string.IsNullOrWhiteSpace(ev.Value))
				throw new SysException(SR.ErrDefaultStorageProviderNotSet);

			DefaultStorageProvider = Types.GetType(ev.Value).CreateInstance<IStorageProvider>();

			return DefaultStorageProvider;
		}

		public List<IStorageProvider> Query()
		{
			return Providers.Values.ToList();
		}
	}
}
