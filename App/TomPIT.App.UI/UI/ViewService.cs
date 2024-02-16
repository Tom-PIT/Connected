using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using TomPIT.App.Models;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Models;
using TomPIT.Runtime;
using TomPIT.UI;

namespace TomPIT.App.UI
{
	internal class ViewService : SynchronizedClientRepository<IConfiguration, Guid>, IViewService
	{
		private Lazy<ConcurrentDictionary<Guid, bool>> _changeState = new Lazy<ConcurrentDictionary<Guid, bool>>();

		public ViewService(ITenant tenant) : base(tenant, "view")
		{
			if (!tenant.GetService<IRuntimeService>().IsHotSwappingSupported)
				return;

			tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;

			tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;

			tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
		}

		private void OnComponentChanged(ITenant sender, ComponentEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			if (IsView(e.Category) && !Tenant.IsMicroServiceSupported(e.MicroService))
				return;

			ChangeState[e.Component] = true;
			Refresh(e.Component);
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			var supportedCategories = "MasterView, Partial, MailTemplate, Report";

			if (Tenant.IsMicroServiceSupported(e.MicroService))
				supportedCategories += ", View";

			var views = Tenant.GetService<IComponentService>().QueryConfigurations(e.MicroService, supportedCategories);

			foreach (var i in views)
				Set(i.Component, i, TimeSpan.Zero);

			//ReplaceLogin();
		}

		private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			if (IsView(e.Category) && !Tenant.IsMicroServiceSupported(e.MicroService))
				return;

			ChangeState[e.Component] = true;

			Remove(e.Component);
			//ReplaceLogin();
		}

		private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			if (IsView(e.Category) && !Tenant.IsMicroServiceSupported(e.MicroService))
				return;

			Refresh(e.Component);
			//ReplaceLogin();
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			if (IsView(e.Category) && !Tenant.IsMicroServiceSupported(e.MicroService))
				return;

			ChangeState[e.Component] = true;
			Refresh(e.Component);

			//ReplaceLogin();
		}

		private bool IsView(string category)
		{
			return string.Compare(category, "View", true) == 0;
		}

		private bool IsTargetCategory(string category)
		{
			return string.Compare(category, "View", true) == 0
				 || string.Compare(category, "MasterView", true) == 0
				 || string.Compare(category, "Partial", true) == 0
				 || string.Compare(category, "MailTemplate", true) == 0
				 || string.Compare(category, "Report", true) == 0;
		}

		protected override void OnInitializing()
		{
			var views = Tenant.GetService<IComponentService>().QueryConfigurations(Tenant.GetService<IResourceGroupService>().QuerySupported().Select(f => f.Name).ToList(), "View");
			var commonViews = Tenant.GetService<IComponentService>().QueryConfigurations("MasterView", "Partial", "MailTemplate", "Report");

			views = views.Union(commonViews).ToImmutableList();

			foreach (var i in views)
			{
				if (i is null || !Tenant.GetService<IRuntimeService>().IsMicroServiceSupported(i.MicroService()))
					continue;

				Set(i.Component, i, TimeSpan.Zero);
			}
		}

		protected override void OnInvalidate(Guid id)
		{
			if (!Tenant.GetService<IRuntimeService>().IsHotSwappingSupported)
				return;

			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(id);

			if (configuration is null)
			{
				Remove(id);
				return;
			}

			if (!Tenant.GetService<IRuntimeService>().IsMicroServiceSupported(configuration.MicroService()))
				return;

			Set(configuration.Component, configuration, TimeSpan.Zero);
		}

		public IViewConfiguration Select(string url, ActionContext context)
		{
			var tokens = url.Split('/');
			var views = Where(f => f is IViewConfiguration);

			foreach (var i in views)
			{
				var view = i as IViewConfiguration;

				if (!view.Enabled || string.IsNullOrWhiteSpace(view.Url))
					continue;

				var template = TemplateParser.Parse(view.Url);
				var path = url.StartsWith('/') ? url : $"/{url}";
				var matcher = new TemplateMatcher(template, null);
				var dictionary = context?.RouteData ?? new RouteData();
				var isMatch = matcher.TryMatch(path, dictionary.Values);

				if (isMatch)
				{
					var component = Tenant.GetService<IComponentService>().SelectComponent(view.Component);
					var microService = Tenant.GetService<IMicroServiceService>().Select(view.MicroService());

					var viewResolutionArgs = new ViewResolutionArgs
					{
						MicroService = microService.Name,
						Name = component.Name,
						Url = url
					};

					var runtimeService = Tenant.GetService<IMicroServiceRuntimeService>();

					if (runtimeService is null)
						return view;

					foreach (var runtime in runtimeService.QueryRuntimes())
					{
						if (runtime?.Resolver?.ResolveView(viewResolutionArgs) is IViewConfiguration config)
							return config;
					}
					return view;
				}
			}

			return null;
		}

		public string SelectContent(IGraphicInterface ui)
		{
			if (ui.TextBlob == Guid.Empty)
				return null;

			return Tenant.GetService<IComponentService>().SelectText(ui.Configuration().MicroService(), ui);
		}

		public bool HasChanged(ViewKind kind, string url)
		{
			if (string.Compare(url, "_ViewImports", true) == 0)
				return false;

			var component = Guid.Empty;
			var snippet = string.Empty;

			switch (kind)
			{
				case ViewKind.Master:
					var master = SelectMaster(url);

					if (master == null)
						return false;

					component = master.Component;
					break;
				case ViewKind.View:
					var view = Select(url, null);

					if (view == null)
						return false;

					component = view.Component;
					break;
				case ViewKind.Partial:
					var partial = SelectPartial(url);

					if (partial == null)
						return false;

					component = partial.Component;
					break;
				case ViewKind.MailTemplate:
					var template = SelectMailTemplate(url);

					if (template == null)
						return false;

					component = template.Component;
					break;
				case ViewKind.Report:
					var report = SelectReport(url);

					if (report == null)
						return false;

					component = report.Component;
					break;
				default:
					break;
			}

			if (component == Guid.Empty)
				return false;

			if (ChangeState.TryGetValue(component, out bool r))
			{
				ChangeState[component] = false;

				return r;
			}
			return false;
		}

		public IConfiguration Select(Guid view)
		{
			return Get(view);
		}

		public IMasterViewConfiguration SelectMaster(string name)
		{
			var tokens = name.Split('/');

			IComponent c;

			if (tokens.Length > 1)
			{
				var s = Tenant.GetService<IMicroServiceService>().Select(tokens[0].Trim());

				if (s == null)
					return null;

				c = Tenant.GetService<IComponentService>().SelectComponent(s.Token, "MasterView", tokens[1].Trim());
			}
			else
				throw new RuntimeException(SR.ErrInvalidQualifier);

			if (c is null)
				return null;

			var masterResolutionArgs = new MasterViewResolutionArgs
			{
				Name = name
			};

			var runtimeService = Tenant.GetService<IMicroServiceRuntimeService>();

			if (runtimeService is not null)
			{
				foreach (var runtime in runtimeService.QueryRuntimes())
				{
					if (runtime?.Resolver?.ResolveMaster(masterResolutionArgs) is IMasterViewConfiguration config)
						return config;
				}
			}

			return Get(c.Token) as IMasterViewConfiguration;
		}

		public IMailTemplateConfiguration SelectMailTemplate(string url)
		{
			var tokens = url.Split('/');

			if (!Guid.TryParse(tokens[0], out Guid _))
				return null;

			var component = Tenant.GetService<IComponentService>().SelectComponent(new Guid(tokens[0]), "MailTemplate", System.IO.Path.GetFileNameWithoutExtension(tokens[1]));

			if (component is null)
				return null;

			var microService = Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

			var mailTemplateResolutionArgs = new MailTemplateResolutionArgs
			{
				MicroService = microService?.Name,
				Name = component?.Name,
				Url = url
			};

			var runtimeService = Tenant.GetService<IMicroServiceRuntimeService>();

			if (runtimeService is not null)
			{
				foreach (var runtime in Tenant.GetService<IMicroServiceRuntimeService>().QueryRuntimes())
				{
					if (runtime?.Resolver?.ResolveMailTemplate(mailTemplateResolutionArgs) is IMailTemplateConfiguration config)
						return config;
				}
			}

			return Get(component.Token) as IMailTemplateConfiguration;
		}

		public IPartialViewConfiguration SelectPartial(string name)
		{
			var tokens = name.Split('/');

			if (tokens.Length == 1)
				return null;

			var s = Tenant.GetService<IMicroServiceService>().Select(tokens[0].Trim());

			if (s is null)
				return null;

			var c = Tenant.GetService<IComponentService>().SelectComponent(s.Token, ComponentCategories.Partial, tokens[1].Trim());

			if (c is null)
				return null;

			var partialResolutionArgs = new PartialViewResolutionArgs
			{
				Name = name
			};

			var runtimeService = Tenant.GetService<IMicroServiceRuntimeService>();

			if (runtimeService is not null)
			{
				foreach (var runtime in Tenant.GetService<IMicroServiceRuntimeService>().QueryRuntimes())
				{
					if (runtime?.Resolver?.ResolvePartial(partialResolutionArgs) is IPartialViewConfiguration config)
						return config;
				}
			}

			return Get(c.Token) as IPartialViewConfiguration;
		}

		public IReportConfiguration SelectReport(string name)
		{
			var tokens = name.Split('/');

			var s = Tenant.GetService<IMicroServiceService>().Select(tokens[tokens.Length - 2].Trim());

			if (s is null)
				return null;

			var reportName = tokens[tokens.Length - 1];

			if (reportName.Contains('.'))
				reportName = Path.GetFileNameWithoutExtension(reportName);

			var c = Tenant.GetService<IComponentService>().SelectComponent(s.Token, "Report", reportName.Trim());

			if (c is null)
				return null;

			var reportResolutionArgs = new ReportResolutionArgs
			{
				Name = name
			};

			var runtimeService = Tenant.GetService<IMicroServiceRuntimeService>();

			if (runtimeService is not null)
			{
				foreach (var runtime in Tenant.GetService<IMicroServiceRuntimeService>().QueryRuntimes())
				{
					if (runtime?.Resolver?.ResolveReport(reportResolutionArgs) is IReportConfiguration config)
						return config;
				}
			}

			return Get(c.Token) as IReportConfiguration;
		}

		private ConcurrentDictionary<Guid, bool> ChangeState => _changeState.Value;

		public ViewKind ResolveViewKind(string url)
		{
			var path = url.Trim('/');

			if (path.StartsWith("Views/Dynamic/Master"))
				return ViewKind.Master;
			else if (path.StartsWith("Views/Dynamic/Partial") || path.StartsWith(":partial"))
				return ViewKind.Partial;
			else if (path.StartsWith("Views/Dynamic/MailTemplate"))
				return ViewKind.MailTemplate;
			else if (path.StartsWith("Views/Dynamic/Report"))
				return ViewKind.Report;
			else
				return ViewKind.View;

		}

		public IMicroService ResolveMicroService(string url)
		{
			var kind = ResolveViewKind(url);

			if (kind == ViewKind.View)
			{
				var path = string.Join('/', url.Trim('/').Split('/').Skip(3));

				if (path.EndsWith(".cshtml"))
					path = path[0..^7];

				var view = Select(path, null);


				if (view == null)
					return null;

				return Tenant.GetService<IMicroServiceService>().Select(view.MicroService());
			}

			var name = url.Trim('/').Split('/')[3];

			return Tenant.GetService<IMicroServiceService>().Select(name);
		}

		public IRuntimeModel CreateModel(IRuntimeModel owner)
		{
			if (owner is RuntimeModel model)
				return new RuntimeModel(model);
			else
				return owner;
		}
	}
}
