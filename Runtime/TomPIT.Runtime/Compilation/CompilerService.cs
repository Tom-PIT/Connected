using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Serialization;

namespace TomPIT.Compilation
{
	internal class CompilerService : ClientRepository<IScriptDescriptor, Guid>, ICompilerService, ICompilerNotification
	{
		public const string ScriptInfoClassName = "__ScriptInfo";

		public event EventHandler<Guid> Invalidated;

		private static readonly Lazy<ConcurrentDictionary<Guid, List<Guid>>> _references = new Lazy<ConcurrentDictionary<Guid, List<Guid>>>();
		private static SingletonProcessor<Guid> _scriptProcessor = new SingletonProcessor<Guid>();
		private NuGetPackages _nuGet;

		public CompilerService(ITenant tenant) : base(tenant, "script")
		{
			Tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;

			Scripts = new ScriptCache(tenant);
		}

		private static ConcurrentDictionary<Guid, List<Guid>> References { get { return _references.Value; } }
		private static SingletonProcessor<Guid> ScriptProcessor => _scriptProcessor;
		internal ScriptCache Scripts { get; }
		public NuGetPackages Nuget => _nuGet ??= new NuGetPackages(Tenant);
		private bool ReadOnly => !Tenant.GetService<IRuntimeService>().IsHotSwappingSupported;

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			var scripts = All();

			foreach (var i in scripts)
			{
				if (i.MicroService == e.MicroService)
					RemoveScript(i.Id);
			}
		}

		private IScriptDescriptor GetCachedScript(Guid sourceCodeId)
		{
			return Get(sourceCodeId);
		}

		public IScriptDescriptor GetScript(CompilerScriptArgs e)
		{
			if (Instance.IsShellMode)
				return null;

			if (!Tenant.GetService<IRuntimeService>().IsMicroServiceSupported(e.MicroService))
				return null;

			if (GetCachedScript(e.SourceCode.Id) is IScriptDescriptor existing)
				return existing;

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

		public Microsoft.CodeAnalysis.Compilation GetCompilation(IText sourceCode)
		{
			if (Instance.IsShellMode)
				return null;

			if (!Tenant.GetService<IRuntimeService>().IsMicroServiceSupported(sourceCode.Configuration().MicroService()))
				return null;

			var microService = sourceCode.Configuration().MicroService();

			using var script = new CompilerScript(Tenant, microService, sourceCode);

			var result = new ScriptDescriptor
			{
				Id = sourceCode.Id,
				MicroService = microService,
				Component = sourceCode.Configuration().Component
			};

			script.Create();

			return Compile(result, script, false, new CompilerScriptArgs(microService, sourceCode));
		}

		private Microsoft.CodeAnalysis.Compilation Compile(IScriptDescriptor script, CompilerScript compiler, bool cache, CompilerScriptArgs e)
		{
			if (compiler.Script is null)
				return null;

			Microsoft.CodeAnalysis.Compilation result;

			var stage = Tenant.GetService<IRuntimeService>().Stage;
			var scriptDescriptor = script as ScriptDescriptor;

			result = compiler.Script.GetCompilation();

			var diagnostics = result.GetDiagnostics();

			scriptDescriptor.Errors = ProcessDiagnostics(script, compiler, diagnostics, stage);

			if (IsValid(script, compiler))
			{
				scriptDescriptor.Script = compiler.Script.CreateDelegate();
				scriptDescriptor.Assembly = result.AssemblyName;
			}

			if (compiler.ScriptReferences is not null && compiler.ScriptReferences.Any())
				References.AddOrUpdate(compiler.SourceCode.Id, compiler.ScriptReferences, (key, oldValue) => oldValue = compiler.ScriptReferences);

			if (cache)
				Set(script.Id, script, TimeSpan.Zero);

			GC.Collect();

			return result;
		}

		private static bool IsValid(IScriptDescriptor descriptor, CompilerScript script)
		{
			return script.Script is not null && !descriptor.Errors.Where(f => f.Severity == DiagnosticSeverity.Error).Any();
		}

		private static List<IDiagnostic> ProcessDiagnostics(IScriptDescriptor script, CompilerScript compiler, ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> errors, EnvironmentStage stage)
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

						if (component is not null)
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
			if (ReadOnly)
				return;

			var id = sourceCode.ScriptId();

			Instance.SysProxy.Development.Notifications.ScriptChanged(microService, component, id);
			RemoveScript(sourceCode.Id);

			try
			{
				Invalidated?.Invoke(this, sourceCode.Id);
			}
			catch { }

			InvalidateReferences(component, id);
		}

		internal static Assembly LoadSystemAssembly(string fileName)
		{
			var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(f => string.Equals(f.ShortName(), fileName, StringComparison.OrdinalIgnoreCase));

			if (asm is not null)
				return asm;

			try
			{
				var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{fileName}.dll");

				if (!File.Exists(assemblyPath))
					return null;

				var name = AssemblyName.GetAssemblyName(assemblyPath);

				asm = Assembly.Load(name);
			}
			catch { return null; }

			return asm;
		}

		public void NotifyChanged(object sender, ScriptChangedEventArgs e)
		{
			if (ReadOnly)
				return;

			RemoveScript(e.SourceCode);

			try
			{
				Invalidated?.Invoke(this, e.SourceCode);
			}
			catch { }

			InvalidateReferences(e.Container, e.SourceCode);
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

		public string CompileView(ITenant tenant, IText sourceCode) => ViewCompiler.Compile(tenant, sourceCode);

		public Type ResolveType(Guid microService, IText sourceCode, string typeName)
		{
			return ResolveType(microService, sourceCode, typeName, true);
		}
		public Type ResolveType(Guid microService, IText sourceCode, string typeName, bool throwException)
		{
			return ScriptTypeResolver.ResolveType(this, microService, sourceCode, typeName, throwException);
		}

		public IScriptContext CreateScriptContext(IText sourceCode)
		{
			return new ScriptContext(Tenant, sourceCode);
		}

		public List<string> QuerySubClasses(IScriptConfiguration script) => ScriptTypeResolver.QuerySubClasses(this, script);

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
		public IComponent ResolveComponent(Type type) => ScriptTypeResolver.ResolveComponent(type);


		public IMicroService ResolveMicroService(object instance)
		{
			if (instance is IMicroServiceObject mo && mo.Context != null && mo.Context.MicroService != null)
				return mo.Context.MicroService;

			return ResolveMicroService(instance.GetType());
		}

		public IMicroService ResolveMicroService(Type type) => ScriptTypeResolver.ResolveMicroService(type);

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
			return Tenant.GetService<IDiscoveryService>().Configuration.Find(path);
		}

		private void RemoveScript(Guid id)
		{
			Remove(id);
			ScriptContext.RemoveContext(id);
		}

		public string Rewrite(string sourceText)
		{
			return NamespaceRewriter.Rewrite(sourceText);
		}
	}
}
