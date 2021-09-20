using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using TomPIT.App.Models;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Models;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;
using TomPIT.Storage;
using TomPIT.UI;

namespace TomPIT.App.UI
{
	internal class ViewService : SynchronizedClientRepository<IConfiguration, Guid>, IViewService
	{
		private Lazy<ConcurrentDictionary<Guid, bool>> _changeState = new Lazy<ConcurrentDictionary<Guid, bool>>();

		public ViewService(ITenant tenant) : base(tenant, "view")
		{
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

			ChangeState[e.Component] = true;
			Refresh(e.Component);
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			if (!Tenant.IsMicroServiceSupported(e.MicroService))
				return;

			var views = Tenant.GetService<IComponentService>().QueryConfigurations(e.MicroService, "View, MasterView, Partial, MailTemplate, Report");

			foreach (var i in views)
				Set(i.Component, i, TimeSpan.Zero);

			//ReplaceLogin();
		}

		private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			ChangeState[e.Component] = true;

			Remove(e.Component);
			//ReplaceLogin();
		}

		private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
			//ReplaceLogin();
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			ChangeState[e.Component] = true;
			Refresh(e.Component);

			//ReplaceLogin();
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
			var views = Tenant.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, "View, MasterView, Partial, MailTemplate, Report");

			foreach (var i in views)
			{
				if (i == null)
					continue;

				Set(i.Component, i, TimeSpan.Zero);
			}
		}

		protected override void OnInvalidate(Guid id)
		{
			if (Tenant.GetService<IComponentService>().SelectConfiguration(id) is IConfiguration ui)
				Set(ui.Component, ui, TimeSpan.Zero);
			else
				Remove(id);
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
				var dictionary = context == null ? new RouteData() : context.RouteData;
				var isMatch = matcher.TryMatch(path, dictionary.Values);

				if (isMatch)
					return view;
			}
			var login = views.Where(f => ((IViewConfiguration)f).Url == "login");

			return null;
		}

		public string SelectContent(IGraphicInterface ui)
		{
			if (ui.TextBlob == Guid.Empty)
				return null;

			var r = Tenant.GetService<IStorageService>().Download(ui.TextBlob);

			if (r == null)
				return null;

			return Encoding.UTF8.GetString(r.Content);
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
			IComponent c = null;

			if (tokens.Length > 1)
			{
				var s = Tenant.GetService<IMicroServiceService>().Select(tokens[0].Trim());

				if (s == null)
					return null;

				c = Tenant.GetService<IComponentService>().SelectComponent(s.Token, "MasterView", tokens[1].Trim());
			}
			else
				throw new RuntimeException(SR.ErrInvalidQualifier);

			if (c == null)
				return null;

			return Get(c.Token) as IMasterViewConfiguration;
		}

		public IMailTemplateConfiguration SelectMailTemplate(string url)
		{
			var tokens = url.Split('/');

			if (!Guid.TryParse(tokens[0], out Guid _))
				return null;

			var component = Tenant.GetService<IComponentService>().SelectComponent(new Guid(tokens[0]), "MailTemplate", System.IO.Path.GetFileNameWithoutExtension(tokens[1]));

			if (component == null)
				return null;

			return Get(component.Token) as IMailTemplateConfiguration;
		}

		public IPartialViewConfiguration SelectPartial(string name)
		{
			var tokens = name.Split('/');
			IComponent c = null;

			if (tokens.Length == 1)
				return null;

			var s = Tenant.GetService<IMicroServiceService>().Select(tokens[0].Trim());

			if (s == null)
				return null;

			c = Tenant.GetService<IComponentService>().SelectComponent(s.Token, ComponentCategories.Partial, tokens[1].Trim());

			if (c == null)
				return null;

			return Get(c.Token) as IPartialViewConfiguration;
		}

		public IReportConfiguration SelectReport(string name)
		{
			var tokens = name.Split('/');
			IComponent c = null;

			var s = Tenant.GetService<IMicroServiceService>().Select(tokens[tokens.Length - 2].Trim());

			if (s == null)
				return null;

			var reportName = tokens[tokens.Length - 1];

			if (reportName.Contains('.'))
				reportName = Path.GetFileNameWithoutExtension(reportName);

			c = Tenant.GetService<IComponentService>().SelectComponent(s.Token, "Report", reportName.Trim());

			if (c == null)
				return null;

			return Get(c.Token) as IReportConfiguration;
		}

		private ConcurrentDictionary<Guid, bool> ChangeState => _changeState.Value;

		//private void ReplaceLogin()
		//{
		//	var exists = Select("login", null);

		//	if (exists == null)
		//		Startup.RouteBuilder.AddSystemLogin();
		//	else
		//		Startup.RouteBuilder.RemoveSystemLogin();
		//}

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
			return new RuntimeModel(owner as RuntimeModel);
		}
	}
}
