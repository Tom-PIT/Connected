using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.App.Resources
{
	internal class ResourceService : ClientRepository<CompiledBundle, string>, IResourceService
	{
		public ResourceService(ITenant tenant) : base(tenant, "bundle")
		{
			tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;

			tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
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
			if (string.Compare(e.Category, "Bundle", true) != 0)
				return;

			var c = Tenant.GetService<IComponentService>().SelectComponent(e.Component);

			if (c == null)
				return;

			Remove(GenerateKey(e.MicroService, c.Name.ToLowerInvariant()));
		}

		public string Bundle(string microService, string name)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(GetType().ShortName(), string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

			var r = Get(GenerateKey(ms.Token, name.ToLowerInvariant()));

			if (r != null)
				return r.Content;

			var svc = Tenant.GetService<IComponentService>();

			var c = svc.SelectComponent(ms.Token, ComponentCategories.ScriptBundle, name);

			if (c == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrBundleNotFound, name));

			if (!(svc.SelectConfiguration(c.Token) is IScriptBundleConfiguration config))
				return null;

			var sb = new StringBuilder();

			foreach (var i in config.Scripts)
				sb.Append(GetSource(i));

			r = new CompiledBundle
			{
				Content = config.Minify
				? Minify(sb.ToString())
				: sb.ToString(),

				Name = name,
				MicroService = ms.Token
			};

			Set(GenerateKey(ms.Token, name.ToLowerInvariant()), r);

			return r.Content;
		}

		private string GetSource(IScriptSource source)
		{
			if (source is IScriptFileSystemSource)
				return GetFileSystemSource(source as IScriptFileSystemSource);
			else if (source is IScriptCodeSource)
				return GetCodeSource(source as IScriptCodeSource);
			else if (source is IScriptUploadSource)
				return GetUploadSource(source as IScriptUploadSource);
			else
				throw new NotSupportedException();
		}

		private string GetUploadSource(IScriptUploadSource d)
		{
			if (d.Blob == Guid.Empty)
				return string.Empty;

			var content = Tenant.GetService<IStorageService>().Download(d.Blob);

			if (content == null || content.Content == null || content.Content.Length == 0)
				return string.Empty;

			return Encoding.UTF8.GetString(content.Content);
		}

		private string GetCodeSource(IScriptCodeSource d)
		{
			return Tenant.GetService<IComponentService>().SelectText(d.Configuration().MicroService(), d);
		}

		private string GetFileSystemSource(IScriptFileSystemSource d)
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
