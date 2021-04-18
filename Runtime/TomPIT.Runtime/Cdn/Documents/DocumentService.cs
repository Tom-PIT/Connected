using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Documents
{
	internal class DocumentService : TenantObject, IDocumentService
	{
		private List<IDocumentProvider> _providers = null;

		public DocumentService(ITenant tenant) : base(tenant)
		{
			foreach (var plugin in Instance.Plugins)
			{
				var providers = plugin.GetDocumentProviders();

				if (providers == null)
					continue;

				Providers.AddRange(providers);
			}
		}

		public IDocumentProvider GetProvider(string name)
		{
			if (Providers.Count == 0)
				return null;

			if (string.IsNullOrWhiteSpace(name))
				return Providers[0];

			return Providers.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);
		}

		private List<IDocumentProvider> Providers
		{
			get
			{
				if (_providers == null)
					_providers = new List<IDocumentProvider>();

				return _providers;
			}
		}

		public IDocumentDescriptor Create(string report, DocumentCreateArgs e)
		{
			using var ctx = MicroServiceContext.FromIdentifier(report, Tenant);
			var descriptor = ComponentDescriptor.Report(ctx, report);

			descriptor.Validate();
			descriptor.ValidateConfiguration();

			var provider = GetProvider(e.Provider);

			return provider.Create(descriptor.Component.Token, e);
		}
	}
}
