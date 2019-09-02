using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Services;
using TomPIT.Storage;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.Http;

namespace TomPIT.UI
{
	internal class ViewService : SynchronizedClientRepository<IConfiguration, Guid>, IViewService
	{
		private Lazy<ConcurrentDictionary<Guid, bool>> _changeState = new Lazy<ConcurrentDictionary<Guid, bool>>();
		private Lazy<ConcurrentDictionary<string, bool>> _snippetState = new Lazy<ConcurrentDictionary<string, bool>>();

		public ViewService(ISysConnection connection) : base(connection, "view")
		{
			connection.GetService<IComponentService>().ComponentChanged += OnComponentChanged;

			connection.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			connection.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			connection.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;

			connection.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;

			ReplaceLogin();
		}

		private void OnComponentChanged(ISysConnection sender, ComponentEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			ChangeState[e.Component] = true;
			Refresh(e.Component);
			SynchronizeSnippetsState(e.Component);

			ReplaceLogin();
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			if (!Connection.IsMicroServiceSupported(e.MicroService))
				return;

			var views = Connection.GetService<IComponentService>().QueryConfigurations(e.MicroService, "View, MasterView, Partial, MailTemplate, Report");

			foreach (var i in views)
				Set(i.Component, i, TimeSpan.Zero);

			ReplaceLogin();
		}

		private void OnConfigurationRemoved(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			ChangeState[e.Component] = true;

			Remove(e.Component);
			SynchronizeSnippetsState(e.Component);
			ReplaceLogin();
		}

		private void OnConfigurationAdded(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
			SynchronizeSnippetsState(e.Component);
			ReplaceLogin();
		}

		private void OnConfigurationChanged(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			ChangeState[e.Component] = true;
			Refresh(e.Component);
			SynchronizeSnippetsState(e.Component);
			ReplaceLogin();
		}

		private void SynchronizeSnippetsState(Guid component)
		{
			try
			{
				var config = Connection.GetService<IComponentService>().SelectConfiguration(component) as ISnippetView;

				if (config == null)
					return;

				foreach (var i in config.Snippets)
					SnippetState[$"{component}/{i.Name}"] = true;
			}
			catch { }
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
			var views = Connection.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, "View, MasterView, Partial, MailTemplate, Report");

			foreach (var i in views)
			{
				if (i == null)
					continue;

				Set(i.Component, i, TimeSpan.Zero);
			}
		}

		protected override void OnInvalidate(Guid id)
		{
			if (Connection.GetService<IComponentService>().SelectConfiguration(id) is IConfiguration ui)
				Set(ui.Component, ui, TimeSpan.Zero);
			else
				Remove(id);
		}

		public IView Select(string url, ActionContext context)
		{
			var tokens = url.Split('/');
			var views = Where(f => f is IView);

			foreach (var i in views)
			{
				var view = i as IView;

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

			return null;
		}

		public string SelectContent(IGraphicInterface ui)
		{
			if (ui.TextBlob == Guid.Empty)
				return null;

			var r = Connection.GetService<IStorageService>().Download(ui.TextBlob);

			if (r == null)
				return null;

			return Encoding.UTF8.GetString(r.Content);
		}

		public bool HasSnippetChanged(ViewKind kind, string url)
		{
			if (string.Compare(url, "_ViewImports", true) == 0)
				return false;

			var tokens = url.Split('.');

			var component = Guid.Empty;
			var snippet = string.Empty;

			switch (kind)
			{
				case ViewKind.Master:
					var masterUrl = url.Split('.');

					var master = SelectMaster(url);

					if (master == null)
						return false;

					component = master.Component;
					snippet = masterUrl[masterUrl.Length - 1];
					break;
				case ViewKind.Partial:
					var partialUrl = url.Split('.');
					var partial = SelectPartial(partialUrl[0]);

					if (partial == null)
						return false;

					component = partial.Component;
					snippet = partialUrl[1];
					break;
				case ViewKind.View:
					var viewUrl = url.Split('.');

					var view = Select(viewUrl[0], null);
					snippet = viewUrl[1];

					if (view == null)
						return false;

					component = view.Component;
					break;
				default:
					break;
			}

			if (component == null)
				return false;

			if (SnippetState.TryGetValue($"{component}/{snippet}", out bool r))
			{
				SnippetState[$"{component}/{snippet}"] = false;

				return r;
			}

			return false;
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

		public IMasterView SelectMaster(string name)
		{
			var tokens = name.Split('.');
			IComponent c = null;

			if (tokens.Length > 1)
			{
				var s = Connection.GetService<IMicroServiceService>().Select(tokens[0].Trim());

				if (s == null)
					return null;

				c = Connection.GetService<IComponentService>().SelectComponent(s.Token, "MasterView", tokens[1].Trim());
			}
			else
				c = Connection.GetService<IComponentService>().SelectComponent("MasterView", name);

			if (c == null)
				return null;

			return Get(c.Token) as IMasterView;
		}

		public IMailTemplate SelectMailTemplate(string url)
		{
			var tokens = url.Split('/');

			if (!Guid.TryParse(tokens[0], out Guid _))
				return null;

			var component = Connection.GetService<IComponentService>().SelectComponent(tokens[0].AsGuid(), "MailTemplate", System.IO.Path.GetFileNameWithoutExtension(tokens[1]));

			if (component == null)
				return null;

			return Get(component.Token) as IMailTemplate;
		}

		public IPartialView SelectPartial(string name)
		{
			var tokens = name.Split('.');
			IComponent c = null;

			if (tokens.Length == 2)
			{
				var s = Connection.GetService<IMicroServiceService>().Select(tokens[0].Trim());

				if (s == null)
					return null;

				c = Connection.GetService<IComponentService>().SelectComponent(s.Token, "Partial", tokens[1].Trim());
			}
			else
				c = Connection.GetService<IComponentService>().SelectComponent("Partial", name);

			if (c == null)
				return null;

			return Get(c.Token) as IPartialView;
		}

		public IReport SelectReport(string name)
		{
			var tokens = name.Split('/');
			IComponent c = null;

			var s = Connection.GetService<IMicroServiceService>().Select(tokens[tokens.Length-2].Trim());

			if (s == null)
				return null;

			var reportName = tokens[tokens.Length - 1];

			if (reportName.Contains('.'))
				reportName = Path.GetFileNameWithoutExtension(reportName);

			c = Connection.GetService<IComponentService>().SelectComponent(s.Token, "Report", reportName.Trim());

			if (c == null)
				return null;

			return Get(c.Token) as IReport;
		}

		private ConcurrentDictionary<Guid, bool> ChangeState => _changeState.Value;
		private ConcurrentDictionary<string, bool> SnippetState => _snippetState.Value;

		private void ReplaceLogin()
		{
			var exists = Select("login", null);

			if (exists == null)
				Startup.RouteBuilder.AddSystemLogin();
			else
				Startup.RouteBuilder.RemoveSystemLogin();
		}
	}
}
