using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.App.Resources
{
    internal class ResourceService : ClientRepository<CompiledBundle, string>, IResourceService
    {
        private const string FromPreprocessorPattern = "\"(.*?)\"";
        private const string FromPreprocessorPatternSingle = "\'(.*?)\'";
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
            if (string.Compare(e.Category, ComponentCategories.ScriptBundle, true) != 0)
                return;

            var c = Tenant.GetService<IComponentService>().SelectComponent(e.Component);

            if (c is null || e.Component != c.Token)
                return;

            var keys = Keys();

            if (keys == null)
                return;

            foreach (var key in keys)
            {
                var tokens = key.Split('.', 3);

                if (new Guid(tokens[0]) == e.MicroService && string.Compare(tokens[1], c.Name.ToLowerInvariant(), true) == 0)
                    Remove(key);
            }
        }

        public string Bundle(string microService, string name)
        {
            var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

            if (ms == null)
                throw new RuntimeException(GetType().ShortName(), string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

            using var ctx = new MicroServiceContext(ms.Token);
            var key = GenerateKey(ms.Token, name.ToLowerInvariant(), ctx.Services.Routing.RootUrl);
            var r = Get(key);

            if (r != null)
                return r.Content;

            var svc = Tenant.GetService<IComponentService>();

            var c = svc.SelectComponent(ms.Token, ComponentCategories.ScriptBundle, name);

            if (c == null)
                throw new RuntimeException(string.Format("{0} ({1})", SR.ErrBundleNotFound, name));

            if (svc.SelectConfiguration(c.Token) is not IScriptBundleConfiguration config)
                return null;

            var sb = new StringBuilder();

            foreach (var i in config.Scripts)
                sb.Append(GetSource(i));

            r = new CompiledBundle
            {
                Content = config.Minify && ms.Status != MicroServiceStatus.Development
                ? Minify(sb.ToString())
                : sb.ToString(),

                Name = name,
                MicroService = ms.Token,
                Url = ctx.Services.Routing.RootUrl
            };

            Set(key, r);

            return r.Content;
        }

        private string GetSource(IScriptSource source)
        {
            if (source is IScriptFileSystemSource)
                return Preprocessor(GetFileSystemSource(source as IScriptFileSystemSource));
            else if (source is IScriptCodeSource)
                return Preprocessor(GetCodeSource(source as IScriptCodeSource));
            else if (source is IScriptUploadSource)
                return Preprocessor(GetUploadSource(source as IScriptUploadSource));
            else
                throw new NotSupportedException();
        }

        private string Preprocessor(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return source;

            var result = new StringBuilder();
            using var reader = new StringReader(source);

            while (reader.Peek() != -1)
            {
                var line = reader.ReadLine();

                if (line.Trim().StartsWith("import", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.Contains('"'))
                        line = Regex.Replace(line, FromPreprocessorPattern, new MatchEvaluator(ProcessPath));
                    else
                        line = Regex.Replace(line, FromPreprocessorPatternSingle, new MatchEvaluator(ProcessPath));
                }

                result.AppendLine(line);
            }

            return result.ToString();
        }

        private string ProcessPath(Match match)
        {
            if (!match.Value.StartsWith("\"@:") && !match.Value.StartsWith("\'@:"))
                return match.Value;

            var path = match.Value[3..^1];
            var tokens = path.Split('/');

            using var ctx = new MiddlewareContext();
            //var ms = ctx.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);
            //var component = ctx.Tenant.GetService<IComponentService>().SelectComponent(ms.Token, ComponentCategories.ScriptBundle, tokens[1]);

            //return $"\"{ctx.Services.Routing.RootUrl}/sys/bundles/{path}?{component.Modified.Ticks}\"";
            return $"\"{ctx.Services.Routing.RootUrl}/sys/bundles/{path}\"";
        }

        private string GetUploadSource(IScriptUploadSource d)
        {
            if (d.Blob == default)
                return string.Empty;

            var content = Tenant.GetService<IStorageService>().Download(d.Blob);

            if ((content?.Content?.Length ?? 0) == 0)
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
            return source;
            //	return new BundleMinifier().Minify(source);
        }
    }
}
