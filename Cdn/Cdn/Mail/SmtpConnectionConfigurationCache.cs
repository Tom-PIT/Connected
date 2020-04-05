using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Services;

namespace TomPIT.Cdn.Mail
{
	internal class SmtpConnectionConfigurationCache : ConfigurationRepository<ISmtpConnectionConfiguration>
	{
		private Lazy<ConcurrentDictionary<Guid, ISmtpConnectionMiddleware>> _middlewares = new Lazy<ConcurrentDictionary<Guid, ISmtpConnectionMiddleware>>();
		public SmtpConnectionConfigurationCache(ITenant tenant) : base(tenant, "smtpconnectionconfiguration")
		{
		}

		protected override string[] Categories => new string[] { ComponentCategories.SmtpConnection };

		protected override void OnChanged(Guid microService, Guid component)
		{
			CreateMiddleware(component);
		}

		protected override void OnInitialized()
		{
			foreach (var config in All())
				CreateMiddleware(config.Component);
		}

		private void CreateMiddleware(Guid configuration)
		{
			if (Middlewares.ContainsKey(configuration))
				Middlewares.TryRemove(configuration, out _);

			var config = Get(configuration);

			if (config == null)
				return;

			var type = Tenant.GetService<ICompilerService>().ResolveType(config.MicroService(), config, config.ComponentName(), false);

			if (type == null)
			{
				Tenant.LogWarning(nameof(SmtpConnectionConfigurationCache), $"{SR.ErrCannotResolveComponentType} ({config.ComponentName()})", LogCategories.Cdn);
				return;
			}

			var instance = Tenant.GetService<ICompilerService>().CreateInstance<ISmtpConnectionMiddleware>(config);

			Middlewares.TryAdd(configuration, instance);
		}

		private ConcurrentDictionary<Guid, ISmtpConnectionMiddleware> Middlewares => _middlewares.Value;

		public ICollection<ISmtpConnectionMiddleware> Handlers
		{
			get
			{
				Initialize();
				return Middlewares.Values;
			}
		}
	}
}
