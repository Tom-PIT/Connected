﻿using dotless.Core;
using dotless.Core.configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Routing;
using TomPIT.Services;
using TomPIT.UI;

namespace TomPIT.Themes
{
	internal class ThemeService : ClientRepository<CompiledTheme, string>, IThemeService
	{
		public ThemeService(ISysConnection connection) : base(connection, "theme")
		{
			connection.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			connection.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			connection.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;
		}

		private void OnConfigurationRemoved(ISysConnection sender, ConfigurationEventArgs e)
		{
			Invalidate(e);
		}

		private void OnConfigurationAdded(ISysConnection sender, ConfigurationEventArgs e)
		{
			Invalidate(e);
		}

		private void OnConfigurationChanged(ISysConnection sender, ConfigurationEventArgs e)
		{
			Invalidate(e);
		}

		private void Invalidate(ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, "Theme", true) != 0)
				return;

			var c = Connection.GetService<IComponentService>().SelectComponent(e.Component);

			if (c == null)
				return;

			Remove(GenerateKey(e.MicroService, c.Name.ToLowerInvariant()));
		}

		public string Compile(string microService, string name)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(GetType().ShortName(), string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

			var r = Get(GenerateKey(ms.Token, name.ToLowerInvariant()));

			if (r != null)
				return r.Content;

			var svc = Connection.GetService<IComponentService>();

			var c = svc.SelectComponent(ms.Token, "Theme", name);

			if (c == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrThemeNotFound, name));

			var config = svc.SelectConfiguration(c.Token) as ITheme;

			if (config == null)
				return null;

			var sb = new StringBuilder();

			foreach (var i in config.Stylesheets)
			{
				string source = string.Empty;

				if (i is IStaticResource)
					source = SelectFileSystemCss(i as IStaticResource);
				else
					source = svc.SelectText(c.MicroService, i as IText);

				if (i is ICssFile || i is IStaticResource)
					sb.Append(source);
				else if (i is ILessFile)
					sb.Append(CompileLess(source));
			}

			r = new CompiledTheme
			{
				Content = Minify(sb.ToString())
			};

			Set(GenerateKey(ms.Token, name.ToLowerInvariant()), r);

			return r.Content;
		}

		private string SelectFileSystemCss(IStaticResource d)
		{
			if (d.VirtualPath == string.Empty)
				return string.Empty;

			var p = d.VirtualPath.StartsWith("~")
				? d.VirtualPath.Substring(1)
				: d.VirtualPath;

			if (!p.StartsWith("/"))
				p = string.Format("/{0}", p);

			var path = string.Format("{0}{1}", Shell.GetService<IRuntimeService>().WebRoot, p.Replace('/', '\\'));

			if (!File.Exists(path))
				return string.Empty;

			return File.ReadAllText(path);

		}

		private string CompileLess(string source)
		{
			var variables = LoadVariablesLess();

			var config = new DotlessConfiguration
			{
				ImportAllFilesAsLess = true,
				MinifyOutput = true,
				Logger = typeof(LessCompileLogger)
			};

			return Less.Parse(string.Format("{0}{1}{2}", variables, System.Environment.NewLine, source), config);
		}

		private object LoadVariablesLess()
		{
			var path = string.Format("{0}{1}", Shell.GetService<IRuntimeService>().WebRoot, @"\Assets\Styles\Variables.less");

			if (!File.Exists(path))
				return string.Empty;

			return File.ReadAllText(path);
		}

		private string Minify(string source)
		{
			source = Regex.Replace(source, @"[a-zA-Z]+#", "#");
			source = Regex.Replace(source, @"[\n\r]+\s*", string.Empty);
			source = Regex.Replace(source, @"\s+", " ");
			source = Regex.Replace(source, @"\s?([:,;{}])\s?", "$1");
			source = source.Replace(";}", "}");
			source = Regex.Replace(source, @"([\s:]0)(px|pt|%|em)", "$1");

			// Remove comments from CSS
			source = Regex.Replace(source, @"/\*[\d\D]*?\*/", string.Empty);

			return source;
		}
	}
}