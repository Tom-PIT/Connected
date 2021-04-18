using System;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;

namespace TomPIT.Data
{
	internal class CommandTextParserCache : ClientRepository<ICommandTextDescriptor, Guid>
	{
		public CommandTextParserCache(ITenant tenant): base(tenant, nameof(CommandTextParserCache))
		{
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			Tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationChanged;
			Tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationChanged;
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Model, true) == 0)
				Remove(e.Component);
		}

		public ICommandTextDescriptor CreateDescriptor(IModelOperation operation, IDataConnection connection)
		{
			var result = Get(operation.Id);

			if (result != null)
				return result;

			var config = operation.Configuration();
			var text = Tenant.GetService<IComponentService>().SelectText(config.MicroService(), operation);

			if (string.IsNullOrWhiteSpace(text))
				throw new RuntimeException("ModelMiddleware", $"{SR.ErrCommandTextNotSet} ({config.ComponentName()}/{operation.Name})", LogCategories.Middleware);

			var parser = connection.Parser;

			if (parser == null)
				throw new RuntimeException("ModelMiddleware", $"{SR.DataProviderParserNull} ({config.ComponentName()}/{operation.Name})", LogCategories.Middleware);

			result = parser.Parse(text);

			Set(operation.Id, result, TimeSpan.Zero);

			return result;
		}
	}
}
