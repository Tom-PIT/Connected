using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.UI.Theming
{
	internal class ThemeFilesProvider : TenantObject
	{
		private Dictionary<IComponent, IThemeConfiguration> _themes = null;
		public ThemeFilesProvider(ITenant tenant, IMicroService microService, string name):base(tenant)
		{
			MicroService = microService;
			Name = name;

			Initialize(microService, name);
		}

		private IMicroService MicroService { get; }
		private string Name { get; }

		private Dictionary<IComponent, IThemeConfiguration> Themes => _themes ??= new Dictionary<IComponent, IThemeConfiguration>();

		private void Initialize(IMicroService microService, string name)
		{
			if (Tenant.GetService<IComponentService>().SelectComponent(microService.Token, ComponentCategories.Theme, name) is not IComponent component)
				return;

			if (Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is not IThemeConfiguration configuration)
				return;

			Themes.Add(component, configuration);

			if (string.IsNullOrWhiteSpace(configuration.BaseTheme))
				return;

			using var ctx = MicroServiceContext.FromIdentifier(configuration.BaseTheme, Tenant);
			var descriptor = ComponentDescriptor.Theme(ctx, configuration.BaseTheme);

			descriptor.Validate();
			descriptor.ValidateConfiguration();

			Initialize(descriptor.MicroService, descriptor.ComponentName);
		}

		public IThemeConfiguration GetTheme(string microService, string name)
		{
			if (Tenant.GetService<IMicroServiceService>().Select(microService) is not IMicroService ms)
				return null;

			return GetTheme(ms.Token, name);
		}
		public IThemeConfiguration GetTheme(Guid microService, string name)
		{
			foreach(var component in Themes)
			{
				if (component.Key.MicroService == microService && string.Compare(component.Key.Name, name, true) == 0)
					return component.Value;
			}

			return null;
		}

		private ImmutableArray<IThemeConfiguration> CreateInheritanceList()
		{
			var theme = GetTheme(MicroService.Token, Name);
			var result = ImmutableArray.Create(theme);

			if (string.IsNullOrWhiteSpace(theme.BaseTheme))
				return result;

			return CreateInheritanceList(result, theme.BaseTheme);
		}

		private ImmutableArray<IThemeConfiguration> CreateInheritanceList(ImmutableArray<IThemeConfiguration> existing, string baseTheme)
		{
			var tokens = baseTheme.Split('/');
			var theme = GetTheme(tokens[0], tokens[1]);
			var result = existing.Insert(0, theme);

			if (string.IsNullOrWhiteSpace(theme.BaseTheme))
				return result;

			return CreateInheritanceList(result, theme.BaseTheme);
		}

		public ThemeFileSet CreateFileSet()
		{
			var chain = CreateInheritanceList();
			var result = new ThemeFileSet();

			chain.ToBuilder().Reverse();

			foreach (var theme in chain)
			{
				foreach (var file in theme.Stylesheets)
					result.Add(file.Name, file);
			}

			return result;
		}
	}
}
