using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.Connectivity;
using TomPIT.Runtime;
using TomPIT.UI.Theming.Configuration;
using TomPIT.UI.Theming.Engine;
using TomPIT.UI.Theming.Importers;
using TomPIT.UI.Theming.Parser;
using TomPIT.UI.Theming.Parser.Infrastructure;
using TomPIT.UI.Theming.Parser.Infrastructure.Nodes;
using TomPIT.UI.Theming.Parser.Tree;
using TomPIT.UI.Theming.Stylizers;

namespace TomPIT.UI.Theming
{
    internal class ThemeParser : TenantObject
    {
        private LessParser _parser;
        private LessConfiguration _configuration;
        private Env _environment;
        private ILessEngine _engine;
        public ThemeParser(ITenant tenant) : base(tenant)
        {
        }

        private LessConfiguration Configuration => _configuration ??= new LessConfiguration
        {
            ImportAllFilesAsLess = false,
            MinifyOutput = true,
            Logger = typeof(LessCompileLogger),
            LogLevel = Loggers.LogLevel.Warn,
        };

        private LessParser Parser => _parser ??= new LessParser(Configuration, new PlainStylizer(), new Importer(new ThemeFileReader(Tenant)));
        private Env Environment => _environment ??= new Env(Parser);
        private ILessEngine Engine => _engine ??= new EngineFactory(Configuration).GetEngine();

        public string Parse(ThemeFileSet files)
        {
            Ruleset rules = null;

            foreach (var file in files.GetFileNames())
            {
                if (Merge(files.GetFiles(file)) is Ruleset rs)
                {
                    if (rules is null)
                        rules = rs;
                    else
                        rules.Rules.AddRange(rs.Rules);
                }
            }

            return rules is null ? string.Empty : rules.ToCSS(Environment);
        }

        private Ruleset Merge(ImmutableArray<IThemeFile> files)
        {
            if (files.IsDefaultOrEmpty)
                return null;

            var baseFile = files.First();
            var text = LoadText(baseFile);

            var baseRule = Parser.Parse(text, baseFile is IText textFile ? textFile.FileName : null);

            foreach (var file in files.Skip(1))
                Merge(baseRule, file);

            return baseRule;//.ToCSS(Environment);
        }

        private void Merge(Ruleset baseRules, IThemeFile file)
        {
            var text = LoadText(file);

            if (string.IsNullOrWhiteSpace(text))
                return;

            var rules = Parser.Parse(text, file is IText textFile ? textFile.FileName : null);

            foreach (var node in rules.Rules)
                SynchronizeNode(baseRules, node);
        }

        private static void SynchronizeNode(Ruleset nodes, Node node)
        {
            if (node is Rule rule)
                SynchronizeRule(nodes, rule);
            else
                nodes.Rules.Add(node);
        }

        private static void SynchronizeRule(Ruleset nodes, Rule rule)
        {
            foreach (var node in nodes.Rules)
            {
                if (node is Rule existingRule && string.Compare(existingRule.Name, rule.Name, true) == 0)
                {
                    existingRule.Value = rule.Value;
                    return;
                }
            }

            nodes.Rules.Add(rule);
        }

        private string LoadText(IThemeFile file)
        {
            if (file is IStaticResource)
                return LoadFromFileSystem(file as IStaticResource);
            else
                return Tenant.GetService<IComponentService>().SelectText(file.Configuration().MicroService(), file as IText);
        }

        private static string LoadFromFileSystem(IStaticResource d)
        {
            if (string.IsNullOrWhiteSpace(d.VirtualPath))
                return string.Empty;

            var p = d.VirtualPath.StartsWith("~")
                ? d.VirtualPath[1..]
                : d.VirtualPath;

            if (!p.StartsWith("/"))
                p = string.Format("/{0}", p);

            var path = string.Format("{0}{1}", Shell.GetService<IRuntimeService>().WebRoot, p.Replace('/', '\\'));

            if (!File.Exists(path))
                return string.Empty;

            return File.ReadAllText(path);
        }
    }
}
