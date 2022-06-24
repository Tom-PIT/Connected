using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.UI.Theming
{
	public class ThemeService : ClientRepository<CompiledTheme, string>, IThemeService
	{
		public ThemeService(ITenant Tenant) : base(Tenant, "theme")
		{
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			Tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			Tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;

			Tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			if (!Tenant.IsMicroServiceSupported(e.MicroService))
				return;

			foreach (var i in All())
			{
				if (i.MicroService == e.MicroService)
					Remove(GenerateKey(i.MicroService, i.Name.ToLowerInvariant()));
			}
		}

		private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			Invalidate(e);
		}

		private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			Invalidate(e);
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			Invalidate(e);
		}

		private void Invalidate(ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, "Theme", true) != 0)
				return;

			var c = Tenant.GetService<IComponentService>().SelectComponent(e.Component);

			if (c == null)
				return;

			Remove(GenerateKey(e.MicroService, c.Name.ToLowerInvariant()));
		}

		public string Compile(string microService, string name)
		{
			if (Tenant.GetService<IMicroServiceService>().Select(microService) is not IMicroService ms)
				throw new RuntimeException(GetType().ShortName(), $"{SR.ErrMicroServiceNotFound} ({microService})");

			if (Get(GenerateKey(ms.Token, name.ToLowerInvariant())) is CompiledTheme existing)
				return existing.Content;

			var compiler = new ThemeCompiler(Tenant, ms, name);
			var compiled = compiler.Compile();

			Set(GenerateKey(ms.Token, name.ToLowerInvariant()), compiled);

			return compiled.Content;
		}
	}
}
