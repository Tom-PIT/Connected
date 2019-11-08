using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.Connectivity;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Services;
using TomPIT.UI.Theming;

namespace TomPIT.Ide.UI.Theming
{
	internal class StylesheetService : ConfigurationRepository<IThemeConfiguration>, IStylesheetService
	{
		private const string StopCharacters = "> [:+(.~";

		private Lazy<List<IStylesheetClass>> _systemClasses = new Lazy<List<IStylesheetClass>>();
		private Lazy<ConcurrentDictionary<Guid, ConcurrentDictionary<string, List<IStylesheetClass>>>> _classes = new Lazy<ConcurrentDictionary<Guid, ConcurrentDictionary<string, List<IStylesheetClass>>>>();

		private CssParser _parser = null;

		public StylesheetService(ITenant tenant) : base(tenant, "stylesheettheme")
		{

		}

		protected override void OnAdded(Guid microService, Guid component)
		{
			Classes.TryRemove(microService, out _);
		}

		protected override void OnChanged(Guid microService, Guid component)
		{
			Classes.TryRemove(microService, out _);
		}

		protected override void OnRemoved(Guid microService, Guid component)
		{
			Classes.TryRemove(microService, out _);
		}

		protected override string[] Categories => new string[] { ComponentCategories.Theme };

		public List<IStylesheetClass> QueryClasses(Guid microService, bool includeDependencies)
		{
			var result = new List<IStylesheetClass>();

			if (SystemClasses.Count > 0)
				result.AddRange(SystemClasses);

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return result;

			FillItems(ms.Name, result);

			if (includeDependencies)
			{
				var references = Tenant.GetService<IDiscoveryService>().References(microService);

				if (references != null)
				{
					foreach (var reference in references.MicroServices)
						FillItems(reference.MicroService, result);
				}
			}

			return result;
		}

		private void FillItems(string microService, List<IStylesheetClass> items)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return;

			ConcurrentDictionary<string, List<IStylesheetClass>> microServiceClasses = null;

			if (!Classes.ContainsKey(ms.Token))
				microServiceClasses = CreateMicroServiceClasses(ms);
			else
				microServiceClasses = Classes[ms.Token];

			foreach (var theme in microServiceClasses)
			{
				if (theme.Value.Count > 0)
					items.AddRange(theme.Value);
			}
		}

		private ConcurrentDictionary<string, List<IStylesheetClass>> CreateMicroServiceClasses(IMicroService microService)
		{
			var result = new ConcurrentDictionary<string, List<IStylesheetClass>>();

			Classes.TryAdd(microService.Token, result);

			foreach (var configuration in All())
			{
				if (configuration.MicroService() != microService.Token)
					continue;

				var name = configuration.ComponentName();
				var compiledTheme = Tenant.GetService<IThemeService>().Compile(microService.Name, name);
				var items = new List<IStylesheetClass>();

				result.TryAdd(name, items);

				if (!string.IsNullOrWhiteSpace(compiledTheme))
				{
					var classes = ParseClasses(compiledTheme);

					if (classes == null || classes.Count == 0)
						continue;

					foreach (var @class in classes)
					{
						items.Add(new StylesheetClass
						{
							MicroService = microService.Name,
							Theme = name,
							Name = @class
						});
					}
				}
			}

			return result;
		}

		private List<IStylesheetClass> SystemClasses => _systemClasses.Value;

		protected override void OnInitializing()
		{
			base.OnInitializing();

			lock (_systemClasses.Value)
			{
				var file = $"{Tenant.GetService<IRuntimeService>().WebRoot}\\Assets\\idemap.min.css";

				if (!File.Exists(file))
					return;

				var classes = ParseClasses(File.ReadAllText(file));

				foreach (var @class in classes)
				{
					var theme = "Bootstrap";

					if (@class.StartsWith("fa"))
						theme = "Awesome";

					SystemClasses.Add(new StylesheetClass
					{
						MicroService = "System",
						Theme = theme,
						Name = @class
					});
				}
			}
		}

		private List<string> ParseClasses(string text)
		{
			var css = Parser.ParseStyleSheet(text) as ICssStyleSheet;
			var classes = new List<string>();

			foreach (var rule in css.Rules)
			{
				if (!(rule is ICssStyleRule style))
					continue;

				if (string.IsNullOrWhiteSpace(style.SelectorText))
					continue;

				if (!style.SelectorText.StartsWith('.'))
					continue;

				if (style.Selector is IEnumerable<ISelector> en)
				{
					foreach (var s in en)
					{
						if (!s.Text.StartsWith('.'))
							continue;

						var classText = ResolveClassName(s.Text);

						if (!classes.Contains(classText))
							classes.Add(classText);
					}
				}
				else
				{
					var className = ResolveClassName(style.SelectorText);

					if (!classes.Contains(className))
						classes.Add(className);
				}
			}

			return classes;
		}

		private string ResolveClassName(string text)
		{
			if (text.StartsWith('.'))
				text = text.Substring(1);

			var index = text.IndexOfAny(StopCharacters.ToCharArray());

			if (index == -1)
				return text;

			return text.Substring(0, index);
		}

		private CssParser Parser
		{
			get
			{
				if (_parser == null)
				{
					_parser = new CssParser(new CssParserOptions
					{
						IsIncludingUnknownDeclarations = true,
						IsIncludingUnknownRules = true,
						IsToleratingInvalidSelectors = true
					});
				}

				return _parser;
			}
		}

		private ConcurrentDictionary<Guid, ConcurrentDictionary<string, List<IStylesheetClass>>> Classes => _classes.Value;
	}
}