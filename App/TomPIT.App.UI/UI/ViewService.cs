using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.UI
{
	internal class ViewService : SynchronizedClientRepository<IGraphicInterface, Guid>, IViewService
	{
		private ViewScriptsCache _scripts = null;
		private Lazy<ConcurrentDictionary<Guid, bool>> _changeState = new Lazy<ConcurrentDictionary<Guid, bool>>();

		public ViewService(ISysConnection connection) : base(connection, "view")
		{
			_scripts = new ViewScriptsCache(connection);

			connection.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			connection.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			connection.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;

			connection.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			if (!Connection.IsMicroServiceSupported(e.MicroService))
				return;

			var views = Connection.GetService<IComponentService>().QueryConfigurations(e.MicroService, string.Format("{0}, {1}, {2}", "View", "MasterView", "Partial"));

			foreach (var i in views)
				Set(i.Component, i as IGraphicInterface, TimeSpan.Zero);
		}

		private void OnConfigurationRemoved(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			ChangeState[e.Component] = true;

			Remove(e.Component);
		}

		private void OnConfigurationAdded(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
		}

		private void OnConfigurationChanged(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			ChangeState[e.Component] = true;

			var c = Connection.GetService<IComponentService>().SelectComponent(e.Component);

			if (c != null)
				ViewScripts.Remove(e.MicroService, c.Token);

			Refresh(e.Component);
		}

		private bool IsTargetCategory(string category)
		{
			return string.Compare(category, "View", true) == 0
				|| string.Compare(category, "MasterView", true) == 0
				|| string.Compare(category, "Partial", true) == 0;
		}

		protected override void OnInitializing()
		{
			var views = Connection.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, string.Format("{0}, {1}, {2}", "View", "MasterView", "Partial"));

			foreach (var i in views)
				Set(i.Component, i as IGraphicInterface, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			if (Connection.GetService<IComponentService>().SelectConfiguration(id) is IGraphicInterface ui)
				Set(((IConfiguration)ui).Component, ui, TimeSpan.Zero);
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
				var targetTokens = view.Url == null ? new string[0] : view.Url.Split('/');

				if (tokens.Length != targetTokens.Length)
					continue;

				bool match = false;
				var routeData = new Dictionary<string, object>();

				for (int j = 0; j < tokens.Length; j++)
				{
					var t1 = tokens[j];
					var t2 = targetTokens[j];

					if (t2.StartsWith("{") && t2.EndsWith("}"))
					{
						if (context != null)
							routeData.Add(t2.Substring(1, t2.Length - 2), t1);

						continue;
					}

					if (string.Compare(t1, t2, true) != 0)
						break;

					match = true;
				}

				if (match)
				{
					foreach (var j in routeData)
						context.RouteData.Values[j.Key] = j.Value;

					return view;
				}
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

		public bool HasChanged(string url)
		{
			var v = Select(url, null);

			if (v == null)
				return true;

			if (ChangeState.TryGetValue(v.Component, out bool r))
			{
				ChangeState[v.Component] = false;

				return r;
			}

			return false;
		}

		public IGraphicInterface Select(Guid view)
		{
			return Get(view);
		}

		public IMasterView SelectMaster(string name)
		{
			var tokens = name.Split('.');
			IComponent c = null;

			if (tokens.Length == 2)
			{
				var s = Connection.GetService<IMicroServiceService>().Select(tokens[0].Trim());

				if (s == null)
					throw new RuntimeException(SR.ErrMicroServiceNotFound);

				c = Connection.GetService<IComponentService>().SelectComponent(s.Token, "MasterView", tokens[1].Trim());
			}
			else
				c = Connection.GetService<IComponentService>().SelectComponent("MasterView", name);

			if (c == null)
				return null;

			return Get(c.Token) as IMasterView;
		}

		public IPartialView SelectPartial(string name)
		{
			var tokens = name.Split('.');
			IComponent c = null;

			if (tokens.Length == 2)
			{
				var s = Connection.GetService<IMicroServiceService>().Select(tokens[0].Trim());

				if (s == null)
					throw new RuntimeException(SR.ErrMicroServiceNotFound);

				c = Connection.GetService<IComponentService>().SelectComponent(s.Token, "Partial", tokens[1].Trim());
			}
			else
				c = Connection.GetService<IComponentService>().SelectComponent("Partial", name);

			if (c == null)
				return null;

			return Get(c.Token) as IPartialView;
		}

		public string SelectScripts(Guid microService, Guid view)
		{
			return ViewScripts.Select(microService, view);
		}

		private ConcurrentDictionary<Guid, bool> ChangeState => _changeState.Value;
		private ViewScriptsCache ViewScripts => _scripts;
	}
}
