using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.Annotations.Design;
using TomPIT.Caching;
using TomPIT.Compilation.Analyzers;
using TomPIT.Compilation.Views;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.Scripting;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.UI;

namespace TomPIT.Compilation
{
    internal class CompilerService : ClientRepository<IScriptDescriptor, Guid>, ICompilerService, ICompilerNotification
    {
        public const string ScriptInfoClassName = "__ScriptInfo";

        public event EventHandler<Guid> Invalidated;

        private static readonly Lazy<ConcurrentDictionary<Guid, List<Guid>>> _references = new Lazy<ConcurrentDictionary<Guid, List<Guid>>>();
        private static SingletonProcessor<Guid> _scriptProcessor = new SingletonProcessor<Guid>();
        //private static List<DiagnosticAnalyzer> _analyzers = null;
        private static object _sync = new object();
        private NuGetPackages _nuGet;

        public CompilerService(ITenant tenant) : base(tenant, "script")
        {
            Tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;

            Scripts = new ScriptCache(tenant);
        }

        private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
        {
            var scripts = All();

            foreach (var i in scripts)
            {
                if (i.MicroService == e.MicroService)
                    RemoveScript(i.Id);
            }
        }

        public NuGetPackages Nuget => _nuGet ??= new NuGetPackages(Tenant);

        private IScriptDescriptor GetCachedScript(Guid sourceCodeId)
        {
            return Get(sourceCodeId);
        }

        public IScriptDescriptor GetScript(CompilerScriptArgs e)
        {
            if (GetCachedScript(e.SourceCode.Id) is IScriptDescriptor existing)
                return existing;

            /*
		 * on staging and production environments we ensure only one script compilation
		 * of the same type can occur at a time. on development environment we allow 
		 * many side by side compilations because analyzers can analyze script that
		 * are compiling and that kind of scenario would cause deadlock in singleton mode.
		 */

            if (Tenant.GetService<IRuntimeService>().Stage == EnvironmentStage.Development || e.IncludeAnalyzers)
                return CreateScript(e);

            IScriptDescriptor script = null;

            ScriptProcessor.Start(e.SourceCode.Id,
                 () =>
                 {
                     script = CreateScript(e);
                 },
                 () =>
                 {
                     script = GetCachedScript(e.SourceCode.Id);
                 });

            return script;
        }

        private Exception UnwrapException<T>(AggregateException ex, T e)
        {
            var sb = new StringBuilder();
            var src = string.Empty;
            var severity = ExceptionSeverity.Critical;
            var stackTrace = new StringBuilder();

            if (ex.InnerException is RuntimeException)
                src = ((RuntimeException)ex.InnerException).Source;

            if (string.IsNullOrWhiteSpace(src) && e != null)
            {
                var att = e.GetType().FindAttribute<ExceptionSourcePropertyAttribute>();

                if (att != null && !string.IsNullOrWhiteSpace(att.PropertyName))
                {
                    var type = e.GetType();

                    while (type != null)
                    {
                        var pi = type.GetProperty(att.PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                        if (pi != null)
                        {
                            src = Types.Convert<string>(pi.GetValue(e));
                            break;
                        }

                        type = type.BaseType;
                    }
                }
            }

            foreach (var i in ex.InnerExceptions)
            {
                sb.AppendLine(i.Message);

                if (string.IsNullOrWhiteSpace(src))
                {
                    src = i.Source;

                    if (i is RuntimeException)
                        severity = ((RuntimeException)i).Severity;
                }

                stackTrace.Append($"{i.StackTrace}{System.Environment.NewLine}");
            }

            var r = new RuntimeException(src, sb.ToString(), stackTrace.ToString())
            {
                Severity = severity,
            };
            //Log.Error(this, r, EventId.CompileError);

            return r;
        }

        private IScriptDescriptor CreateScript(CompilerScriptArgs e)
        {
            using var script = new CompilerScript(Tenant, e.MicroService, e.SourceCode);

            var result = new ScriptDescriptor
            {
                Id = e.SourceCode.Id,
                MicroService = e.MicroService,
                Component = e.SourceCode.Configuration().Component
            };

            script.Create();

            Compile(result, script, true, e);

            return result;
        }

        private IScriptDescriptor CreateScript<T>(Guid microService, IText d)
        {
            using var script = new CompilerGenericScript<T>(Tenant, microService, d);

            var result = new ScriptDescriptor
            {
                Id = d.Id,
                MicroService = microService,
                Component = d.Configuration().Component
            };

            script.Create();

            Compile(result, script, true, new CompilerScriptArgs(microService, d, false));

            return result;
        }

        public Microsoft.CodeAnalysis.Compilation GetCompilation(IText sourceCode)
        {
            var microService = sourceCode.Configuration().MicroService();

            using var script = new CompilerScript(Tenant, microService, sourceCode);

            var result = new ScriptDescriptor
            {
                Id = sourceCode.Id,
                MicroService = microService,
                Component = sourceCode.Configuration().Component
            };

            script.Create();

            return Compile(result, script, false, new CompilerScriptArgs(microService, sourceCode, false));
        }

        private Microsoft.CodeAnalysis.Compilation Compile(IScriptDescriptor script, CompilerScript compiler, bool cache, CompilerScriptArgs e)
        {
            if (compiler.Script == null)
                return null;

            Microsoft.CodeAnalysis.Compilation result;

            var stage = Tenant.GetService<IRuntimeService>().Stage;
            var scriptDescriptor = script as ScriptDescriptor;

            if (stage == EnvironmentStage.Production || !e.IncludeAnalyzers)
            {
                result = compiler.Script.GetCompilation();

                var diagnostics = result.GetDiagnostics();

                scriptDescriptor.Errors = ProcessDiagnostics(script, compiler, diagnostics, stage);
            }
            else
            {
                //if state queue and development no analyzers
                var c = compiler.Script.GetCompilation().WithAnalyzers(GetAnalyzers(CompilerLanguage.CSharp, script.MicroService, script.Component, script.Id));

                result = c.Compilation;

                scriptDescriptor.Errors = ProcessDiagnostics(script, compiler, c.GetAllDiagnosticsAsync().Result, stage);
            }

            if (IsValid(script, compiler))
            {
                scriptDescriptor.Script = compiler.Script.CreateDelegate();
                scriptDescriptor.Assembly = result.AssemblyName;
            }

            if (compiler.ScriptReferences != null && compiler.ScriptReferences.Count > 0)
                References.AddOrUpdate(compiler.SourceCode.Id, compiler.ScriptReferences, (key, oldValue) => oldValue = compiler.ScriptReferences);

            if (cache)
                Set(script.Id, script, TimeSpan.Zero);

            GC.Collect();

            return result;
        }

        private static bool IsValid(IScriptDescriptor descriptor, CompilerScript script)
        {
            return script.Script != null && !descriptor.Errors.Where(f => f.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Any();
        }

        private List<IDiagnostic> ProcessDiagnostics(IScriptDescriptor script, CompilerScript compiler, ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> errors, EnvironmentStage stage)
        {
            var diagnostics = new List<IDiagnostic>();

            foreach (var error in errors)
            {
                if (stage == EnvironmentStage.Production && error.Severity != DiagnosticSeverity.Error)
                    continue;

                var diagnostic = new Diagnostic
                {
                    Message = error.GetMessage(),
                    Severity = error.Severity,
                };

                var position = error.Location.GetMappedLineSpan();

                diagnostic.StartLine = position.StartLinePosition.Line;
                diagnostic.StartColumn = position.StartLinePosition.Character;

                diagnostic.EndLine = position.EndLinePosition.Line;
                diagnostic.EndColumn = position.EndLinePosition.Character;
                diagnostic.Code = error.Id;

                var filePath = error.Location.SourceTree?.FilePath;

                if (!string.IsNullOrWhiteSpace(filePath) && !filePath.Contains('/'))
                {
                    var ms = compiler.Tenant.GetService<IMicroServiceService>().Select(script.MicroService);

                    if (ms != null)
                    {
                        var component = compiler.Tenant.GetService<IComponentService>().SelectComponent(script.Component);

                        if (component != null)
                            filePath = $"{ms.Name}/{component.Name}/{filePath}";
                    }
                }

                diagnostic.Source = error.Location.SourceTree?.FilePath;
                diagnostic.SourcePath = filePath;
                diagnostics.Add(diagnostic);
            }

            return diagnostics;
        }

        public void Invalidate(IMicroServiceContext context, Guid microService, Guid component, IText sourceCode)
        {
            var id = sourceCode.ScriptId();

            Instance.SysProxy.Development.Notifications.ScriptChanged(microService, component, id);
            RemoveScript(sourceCode.Id);

            try
            {
                Invalidated?.Invoke(this, sourceCode.Id);
            }
            catch { }

            InvalidateReferences(component, id);

            if (Tenant.GetService<IDiscoveryService>().Manifests is IManifestDiscoveryNotification notification)
            {
                if (sourceCode.GetType().FindAttribute<SyntaxAttribute>() is not SyntaxAttribute sa || string.Compare(sa.Syntax, SyntaxAttribute.CSharp, true) == 0)
                    notification.Invalidate(microService, component, sourceCode.Id);
            }
        }

        internal static Assembly LoadSystemAssembly(string fileName)
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(f => string.Equals(f.ShortName(), fileName, StringComparison.OrdinalIgnoreCase));

            if (asm != null)
            {
                return asm;
            }

            try
            {
                var name = AssemblyName.GetAssemblyName(string.Format(@"{0}\{1}.dll", AppDomain.CurrentDomain.BaseDirectory, fileName));

                asm = Assembly.Load(name);
            }
            catch { return null; }

            return asm;
        }

        public void NotifyChanged(object sender, ScriptChangedEventArgs e)
        {
            RemoveScript(e.SourceCode);

            try
            {
                Invalidated?.Invoke(this, e.SourceCode);
            }
            catch { }

            InvalidateReferences(e.Container, e.SourceCode);

            try
            {
                if (Tenant.GetService<IDiscoveryService>().Manifests is IManifestDiscoveryNotification notification)
                    notification.NotifyChanged(e.MicroService, e.Container, e.SourceCode);
            }
            catch { }
        }

        private void InvalidateReferences(Guid container, Guid script)
        {
            foreach (var reference in References)
            {
                if (reference.Value.Contains(container) || reference.Value.Contains(script))
                {
                    RemoveScript(reference.Key);

                    try
                    {
                        Invalidated?.Invoke(this, reference.Key);
                    }
                    catch { }

                    References.Remove(reference.Key, out _);
                }
            }
        }

        public string CompileView(ITenant tenant, IText sourceCode)
        {
            var config = sourceCode.Configuration() as IConfiguration;
            var cmp = tenant.GetService<IComponentService>().SelectComponent(config.Component);

            if (cmp == null)
                return null;

            var rendererAtt = config.GetType().FindAttribute<ViewRendererAttribute>();
            var content = string.Empty;

            if (rendererAtt != null)
            {
                var renderer = (rendererAtt.Type ?? Reflection.TypeExtensions.GetType(rendererAtt.TypeName)).CreateInstance<IViewRenderer>();

                content = renderer.CreateContent(tenant, cmp);
            }
            else if (sourceCode.TextBlob != Guid.Empty)
            {
                var r = Tenant.GetService<IStorageService>().Download(sourceCode.TextBlob);

                if (r == null)
                    return null;

                content = Encoding.UTF8.GetString(r.Content);
            }

            ProcessorBase processor = null;

            if (config is IMasterViewConfiguration master)
                processor = new MasterProcessor(master, content);
            else if (config is IViewConfiguration view)
                processor = new ViewProcessor(view, content);
            else if (config is IPartialViewConfiguration partial)
                processor = new PartialProcessor(partial, content);
            else if (config is IMailTemplateConfiguration mail)
                processor = new MailTemplateProcessor(mail, content);
            else if (config is IReportConfiguration report)
                processor = new ReportProcessor(report);

            if (processor == null)
                return null;

            processor.Compile(tenant, cmp, config as IConfiguration);

            return processor.Result;
        }

        public Type ResolveType(Guid microService, IText sourceCode, string typeName)
        {
            return ResolveType(microService, sourceCode, typeName, true);
        }
        public Type ResolveType(Guid microService, IText sourceCode, string typeName, bool throwException)
        {
            var script = GetScript(new CompilerScriptArgs(microService, sourceCode, false));

            if (script == null)
            {
                if (throwException)
                    throw new RuntimeException($"{SR.ErrTypeNotFound} ({typeName})");
                else
                    return null;
            }

            if (script != null && script.Assembly == null && script.Errors != null && script.Errors.Count(f => f.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error) > 0)
            {
                if (throwException)
                    throw new CompilerException(Tenant, script, sourceCode);
                else
                    return null;
            }

            var result = ResolveTypeName(script.Assembly, sourceCode, typeName);

            if (result is null)
            {
                if (throwException)
                    throw new RuntimeException($"{SR.ErrTypeNotFound} ({typeName})");
                else
                    return null;
            }

            return result;
        }

        private Type ResolveTypeName(string assembly, IText sourceCode, string typeName)
        {
            var ns = ResolveNamespace(sourceCode);
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(f => string.Compare(f.ShortName(), assembly, true) == 0);

            if (asm is null)
                return null;

            if (ns is not null)
                typeName = $"{ns.Namespace}.{typeName}";

            if (!typeName.Contains("."))
                return asm.GetTypes().FirstOrDefault(f => string.Compare(f.Name, typeName, true) == 0);

            var tokens = typeName.Split('.');
            var fullTypeName = new StringBuilder();

            fullTypeName.Append("Submission#0");

            foreach (var token in tokens)
                fullTypeName.Append($"+{token}");

            var results = asm.GetTypes().Where(f => string.Compare(f.FullName, fullTypeName.ToString(), true) == 0);

            if (results.Count() > 1)
            {
                var ms = Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());

                throw new RuntimeException($"{SR.ErrTypeMultipleMatch} ({ms.Name}/{sourceCode.Configuration().ComponentName()}/{typeName})");
            }
            else if (results.Count() == 0)
            {
                throw new RuntimeException($"{SR.ErrTypeNotFound} ({typeName})");
            }

            return results.First();
        }

        private INamespaceElement ResolveNamespace(IText sourceCode)
        {
            if (sourceCode is INamespaceElement nse && !string.IsNullOrWhiteSpace(nse.Namespace))
                return nse;

            IElement current = sourceCode;

            while (current is not null)
            {
                current = current.Parent?.Closest<INamespaceElement>();

                if (current is null)
                    break;

                if (!string.IsNullOrWhiteSpace(((INamespaceElement)current).Namespace))
                    return current as INamespaceElement;
            }

            return null;
        }
        public IScriptContext CreateScriptContext(IText sourceCode)
        {
            return new ScriptContext(Tenant, sourceCode);
        }

        public List<string> QuerySubClasses(IScriptConfiguration script)
        {
            var result = new List<string>();

            var scriptType = ResolveType(script.MicroService(), script, script.ComponentName());

            if (scriptType == null)
                return null;

            foreach (var item in Scripts.Items)
            {
                var ms = item.MicroService();
                var name = item.ComponentName();

                var type = ResolveType(ms, item, name, false);

                if (type == null)
                    continue;

                type = type.BaseType;

                while (type != null)
                {
                    if (ScriptTypeComparer.Compare(scriptType, type))
                    {
                        var microService = Tenant.GetService<IMicroServiceService>().Select(ms);

                        if (microService == null)
                            continue;

                        result.Add($"{microService.Name}/{name}");
                        break;
                    }
                }
            }

            return result;
        }

        public T CreateInstance<T>(IMicroServiceContext context, IText sourceCode) where T : class
        {
            return CreateInstance<T>(context, sourceCode, null, sourceCode.Configuration().ComponentName());
        }
        public T CreateInstance<T>(IText sourceCode) where T : class
        {
            return CreateInstance<T>(null, sourceCode);
        }

        public T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, string arguments, string typeName) where T : class
        {
            var ms = Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());

            if (ms == null)
                throw new RuntimeException(SR.ErrMicroServiceNotFound);

            return CreateInstance<T>(context, sourceCode, ms, arguments, typeName);
        }

        public T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, object arguments, string typeName) where T : class
        {
            var ms = Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());

            if (ms == null)
                throw new RuntimeException(SR.ErrMicroServiceNotFound);

            return CreateInstance<T>(context, sourceCode, ms, arguments, typeName);
        }

        public T CreateInstance<T>(IText sourceCode, string arguments, string typeName) where T : class
        {
            return CreateInstance<T>(null, sourceCode, arguments, typeName);
        }

        public T CreateInstance<T>(IText sourceCode, object arguments, string typeName) where T : class
        {
            return CreateInstance<T>(null, sourceCode, arguments, typeName);
        }

        public T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, string arguments) where T : class
        {
            return CreateInstance<T>(context, sourceCode, arguments, sourceCode.Configuration().ComponentName());
        }

        public T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, object arguments) where T : class
        {
            return CreateInstance<T>(context, sourceCode, arguments, sourceCode.Configuration().ComponentName());
        }

        public T CreateInstance<T>(IText sourceCode, string arguments) where T : class
        {
            return CreateInstance<T>(null, sourceCode, arguments, sourceCode.Configuration().ComponentName());
        }

        public T CreateInstance<T>(IText sourceCode, object arguments) where T : class
        {
            return CreateInstance<T>(null, sourceCode, arguments, sourceCode.Configuration().ComponentName());
        }

        public T CreateInstance<T>(IMicroServiceContext context, Type scriptType) where T : class
        {
            return CreateInstance<T>(context, scriptType, null);
        }
        public T CreateInstance<T>(IMicroServiceContext context, Type scriptType, string arguments) where T : class
        {
            return CreateInstance<T>(context, scriptType, context.MicroService, arguments);
        }

        public T CreateInstance<T>(IMicroServiceContext context, Type scriptType, object arguments) where T : class
        {
            return CreateInstance<T>(context, scriptType, context.MicroService, arguments);
        }

        private T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, IMicroService microService, string arguments, string typeName) where T : class
        {
            var instanceType = ResolveType(microService.Token, sourceCode, typeName);

            if (instanceType == null)
                return default;

            return CreateInstance<T>(context, instanceType, microService, arguments);
        }

        private T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, IMicroService microService, object arguments, string typeName) where T : class
        {
            var instanceType = ResolveType(microService.Token, sourceCode, typeName);

            if (instanceType == null)
                return default;

            return CreateInstance<T>(context, instanceType, microService, arguments);
        }

        private T CreateInstance<T>(IMicroServiceContext context, Type scriptType, IMicroService microService, object arguments) where T : class
        {
            if (CreateInstance<T>(scriptType) is T instance)
            {
                InitializeInstance(context, microService, instance, arguments);

                return instance;
            }

            return default;
        }

        private T CreateInstance<T>(IMicroServiceContext context, Type scriptType, IMicroService microService, string arguments) where T : class
        {
            if (CreateInstance<T>(scriptType) is T instance)
            {
                InitializeInstance(context, microService, instance, arguments);

                return instance;
            }

            return default;
        }

        private static T CreateInstance<T>(Type scriptType) where T : class
        {
            if (scriptType is null)
                return default;

            return scriptType.CreateInstance<T>();
        }

        private void InitializeInstance<T>(IMicroServiceContext context, IMicroService microService, T instance, object arguments)
        {
            if (arguments is not null)
            {
                if (Marshall.NeedsMarshalling(instance, arguments.GetType(), ResolverStrategy.SkipRoot))
                {
                    InitializeInstance(context, microService, instance, Serializer.Serialize(arguments));
                    return;
                }

                Marshall.Merge(arguments, instance);
            }

            if (instance is IMiddlewareObject mo)
                mo.SetContext(context ?? new MicroServiceContext(microService));
        }

        private void InitializeInstance<T>(IMicroServiceContext context, IMicroService microService, T instance, string arguments)
        {
            if (arguments is not null)
                Serializer.Populate(arguments, instance);

            if (instance is IMiddlewareObject mo)
                mo.SetContext(context ?? new MicroServiceContext(microService));
        }

        public IComponent ResolveComponent(object instance)
        {
            if (instance == null)
                return null;

            return ResolveComponent(instance.GetType());
        }
        public IComponent ResolveComponent(Type type)
        {
            if (type == null)
                return null;

            var typeInfo = CompilerExtensions.ResolveScriptInfoType(type.Assembly);

            if (typeInfo == null)
                return null;

            var sourceTypes = (List<SourceTypeDescriptor>)typeInfo.GetProperty("SourceTypes").GetValue(null);

            var componentMatch = sourceTypes.FirstOrDefault(typeDescriptor => TypeMatches(typeDescriptor, type));

            if (componentMatch is not null)
            {
                var cmp = Tenant.GetService<IComponentService>().SelectComponent(componentMatch.Component);

                if (cmp is not null)
                    return cmp;
            }

            var component = (Guid)typeInfo.GetProperty("Component").GetValue(null);

            return Tenant.GetService<IComponentService>().SelectComponent(component);
        }

        private bool TypeMatches(SourceTypeDescriptor typeDescriptor, Type type)
        {
            return NameMatches(typeDescriptor.TypeName, type.Name) && NamespaceMatches(type.DeclaringType.FullName, typeDescriptor.ContainingType);
        }

        private bool NameMatches(string name1, string name2)
        {
            return string.Compare(name1, name2) == 0;
        }

        private bool NamespaceMatches(string namespace1, string namespace2)
        {
            var elements1 = namespace1.Split('.', '+').SkipWhile(e => e.StartsWith("Submission#"));
            var elements2 = namespace2.Split('.', '+').SkipWhile(e => e.StartsWith("Submission#"));

            return elements1.SequenceEqual(elements2);
        }

        public IMicroService ResolveMicroService(object instance)
        {
            if (instance is IMicroServiceObject mo && mo.Context != null && mo.Context.MicroService != null)
                return mo.Context.MicroService;

            return ResolveMicroService(instance.GetType());
        }

        public IMicroService ResolveMicroService(Type type)
        {
            if (type == null)
                return null;

            var typeInfo = type.Assembly.DefinedTypes.FirstOrDefault(f => string.Compare(f.Name, ScriptInfoClassName, false) == 0);

            if (typeInfo == null)
                return null;

            var ms = (Guid)typeInfo.GetProperty("MicroService").GetValue(null);

            return Tenant.GetService<IMicroServiceService>().Select(ms);
        }

        public string ResolveReference(Guid microService, string path)
        {
            var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

            if (ms == null)
                return path;

            var resolver = new ScriptResolver(Tenant, microService);

            return resolver.ResolveReference(path, ms.Name);
        }
        public IText ResolveText(Guid microService, string path)
        {
            var resolver = new ScriptResolver(Tenant, microService);

            return resolver.LoadScript(path);
        }

        private static ConcurrentDictionary<Guid, List<Guid>> References { get { return _references.Value; } }
        private static SingletonProcessor<Guid> ScriptProcessor => _scriptProcessor;
        private ScriptCache Scripts { get; }

        public ImmutableArray<DiagnosticAnalyzer> GetAnalyzers(CompilerLanguage language, Guid microService, Guid component, Guid script)
        {
            switch (language)
            {
                case CompilerLanguage.CSharp:
                    return CreateCsAnalyzers(Tenant, microService, component, script);
                case CompilerLanguage.Razor:
                    return CreateRazorAnalyzers(Tenant, microService, component, script);
                default:
                    throw new NotSupportedException();
            }
        }
        private static ImmutableArray<DiagnosticAnalyzer> CreateCsAnalyzers(ITenant tenant, Guid microService, Guid component, Guid script)
        {
            return new List<DiagnosticAnalyzer>
                {
                     new ClassComplianceCodeAnalyer(tenant, microService, component, script),
                     new NamespacingAnalyzer(tenant, microService, component, script),
                     new AttributeAnalyzer(tenant, microService, component, script),
                     new ScriptReferenceAnalyzer(tenant, microService, component, script),
                     new Analyzers.ComponentSpecific.DistributedOperationAnalyzer(tenant, microService, component, script)
                }.ToImmutableArray();
        }

        private static ImmutableArray<DiagnosticAnalyzer> CreateRazorAnalyzers(ITenant tenant, Guid microService, Guid component, Guid script)
        {
            return new List<DiagnosticAnalyzer>
                {
                     new AttributeAnalyzer(tenant, microService, component, script)
                }.ToImmutableArray();
        }

        private void RemoveScript(Guid id)
        {
            Remove(id);
            ScriptContext.RemoveContext(id);
        }
    }
}
