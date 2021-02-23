using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json.Linq;
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
		internal const string ScriptInfoClassName = "__ScriptInfo";

		private static readonly Lazy<ConcurrentDictionary<Guid, List<Guid>>> _references = new Lazy<ConcurrentDictionary<Guid, List<Guid>>>();
		private static Lazy<ConcurrentDictionary<Guid, ManualResetEvent>> _scriptCreateState = new Lazy<ConcurrentDictionary<Guid, ManualResetEvent>>();
		//private static List<DiagnosticAnalyzer> _analyzers = null;
		private static object _sync = new object();

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

		private IScriptDescriptor GetCachedScript(Guid sourceCodeId)
		{
			return Get(sourceCodeId);
		}

		public IScriptDescriptor GetScript<T>(Guid microService, IText sourceCode)
		{
			var d = GetCachedScript(sourceCode.Id);

			if (d == null)
			{
				var re = new ManualResetEvent(false);

				if (!ScriptCreateState.TryAdd(sourceCode.Id, re))
				{
					re.Dispose();
					re = ScriptCreateState[sourceCode.Id];

					re.WaitOne();

					d = GetCachedScript(sourceCode.Id);
				}
				else
				{
					try
					{
						d = CreateScript<T>(microService, sourceCode);
					}
					finally
					{
						re.Set();

						if (ScriptCreateState.TryRemove(sourceCode.Id, out ManualResetEvent e))
							e.Dispose();
					}
				}
			}

			return d;
		}

		public IScriptDescriptor GetScript(Guid microService, IText sourceCode)
		{
			var d = GetCachedScript(sourceCode.Id);

			if (d == null)
			{
				var re = new ManualResetEvent(false);

				if (!ScriptCreateState.TryAdd(sourceCode.Id, re))
				{
					re.Dispose();
					re = ScriptCreateState[sourceCode.Id];

					re.WaitOne();

					d = GetCachedScript(sourceCode.Id);
				}
				else
				{
					try
					{
						d = CreateScript(microService, sourceCode);
					}
					finally
					{
						re.Set();

						if (ScriptCreateState.TryRemove(sourceCode.Id, out ManualResetEvent e))
							e.Dispose();
					}
				}
			}

			return d;
		}

		public object Execute<T>(Guid microService, IText sourceCode, object sender, T e)
		{
			return Execute(microService, sourceCode, sender, e, out bool handled);
		}

		public object Execute<T>(Guid microService, IText sourceCode, object sender, T e, out bool handled)
		{
			handled = false;

			if (sourceCode.TextBlob == Guid.Empty)
				return null;

			var script = GetScript<T>(microService, sourceCode);

			if (script == null)
				return null;

			handled = true;

			if (script.Errors != null && script.Errors.Where(f => f.Severity == DiagnosticSeverity.Error).Count() > 0)
				throw new CompilerException(Tenant, script, sourceCode);

			var globals = new ScriptGlobals<T>
			{
				sender = sender,
				e = e
			};

			try
			{
				return script.Script(globals).Result;
			}
			catch (AggregateException ex)
			{
				throw UnwrapException(ex, e);
			}
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

		private IScriptDescriptor CreateScript(Guid microService, IText d)
		{
			using var script = new CompilerScript(Tenant, microService, d);

			var result = new ScriptDescriptor
			{
				Id = d.Id,
				MicroService = microService,
				Component = d.Configuration().Component
			};

			script.Create();

			Compile(result, script, true);

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

			Compile(result, script, true);

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

			return Compile(result, script, false);
		}



		private Microsoft.CodeAnalysis.Compilation Compile(IScriptDescriptor script, CompilerScript compiler, bool cache)
		{
			Microsoft.CodeAnalysis.Compilation result = null;

			var errors = ImmutableArray<Microsoft.CodeAnalysis.Diagnostic>.Empty;

			if (compiler.Script != null)
				errors = compiler.Script.GetCompilation().WithAnalyzers(CreateAnalyzers(compiler.Tenant, script)).GetAllDiagnosticsAsync().Result;

			var diagnostics = new List<IDiagnostic>();

			foreach (var error in errors)
			{
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


			((ScriptDescriptor)script).Errors = diagnostics;

			if (compiler.Script != null && script.Errors.Where(f => f.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Count() == 0)
			{
				((ScriptDescriptor)script).Script = compiler.Script.CreateDelegate();
				result = compiler.Script.GetCompilation();
				((ScriptDescriptor)script).Assembly = result.AssemblyName;
				var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(f => f.FullName == result.Assembly.Identity.GetDisplayName());

				if (asm != null)
				{
					var loadContext = AssemblyLoadContext.GetLoadContext(asm);

					if (loadContext != null)
						loadContext.Resolving += OnResolving;
				}
			}

			if (compiler.ScriptReferences != null && compiler.ScriptReferences.Count > 0)
				References.AddOrUpdate(compiler.SourceCode.Id, compiler.ScriptReferences, (key, oldValue) => oldValue = compiler.ScriptReferences);

			if (cache)
				Set(script.Id, script, TimeSpan.Zero);

			return result;
		}

		private Assembly OnResolving(AssemblyLoadContext arg1, AssemblyName arg2)
		{
			return null;
		}

		public void Invalidate(IMicroServiceContext context, Guid microService, Guid component, IText sourceCode)
		{
			var u = context.Tenant.CreateUrl("NotificationDevelopment", "ScriptChanged");
			var id = sourceCode.ScriptId();

			var args = new JObject
			{
				{ "microService", microService },
				{ "container", component },
				{ "sourceCode", id }
			};

			Tenant.Post(u, args);
			RemoveScript(sourceCode.Id);
			InvalidateReferences(component, id);
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
			InvalidateReferences(e.Container, e.SourceCode);
		}

		private void InvalidateReferences(Guid container, Guid script)
		{
			foreach (var reference in References)
			{
				if (reference.Value.Contains(container) || reference.Value.Contains(script))
				{
					RemoveScript(reference.Key);
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
			var script = GetScript(microService, sourceCode);

			if (script == null)
			{
				if (throwException)
					throw new RuntimeException($"{SR.ErrTypeNotFound} ({typeName})");
				else
					return null;
			}

			if (script != null && script.Assembly == null && script.Errors.Count(f => f.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error) > 0)
				throw new CompilerException(Tenant, script, sourceCode);

			var target = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(f => string.Compare(f.ShortName(), script.Assembly, true) == 0);
			var result = target?.GetTypes().FirstOrDefault(f => string.Compare(f.Name, typeName, true) == 0);

			if (result == null)
			{
				if (throwException)
					throw new RuntimeException($"{SR.ErrTypeNotFound} ({typeName})");
				else
					return null;
			}

			return result;
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
		public T CreateInstance<T>(IText sourceCode, string arguments, string typeName) where T : class
		{
			return CreateInstance<T>(null, sourceCode, arguments, typeName);
		}

		public T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, string arguments) where T : class
		{
			return CreateInstance<T>(context, sourceCode, arguments, sourceCode.Configuration().ComponentName());
		}
		public T CreateInstance<T>(IText sourceCode, string arguments) where T : class
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

		private T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, IMicroService microService, string arguments, string typeName) where T : class
		{
			var instanceType = ResolveType(microService.Token, sourceCode, typeName);

			if (instanceType == null)
				return default;

			return CreateInstance<T>(context, instanceType, microService, arguments);
		}

		private T CreateInstance<T>(IMicroServiceContext context, Type scriptType, IMicroService microService, string arguments) where T : class
		{
			if (scriptType == null)
				return default;

			var instance = scriptType.CreateInstance<T>();

			if (instance == null)
				return default;

			if (arguments != null)
				Serializer.Populate(arguments, instance);

			if (instance is IMiddlewareObject mo)
				mo.SetContext(context ?? new MicroServiceContext(microService, Tenant.Url));

			return instance;
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

			var typeInfo = type.Assembly.DefinedTypes.FirstOrDefault(f => string.Compare(f.Name, ScriptInfoClassName, false) == 0);

			if (typeInfo == null)
				return null;

			var sourceFiles = (List<SourceFileDescriptor>)typeInfo.GetProperty("SourceFiles").GetValue(null);

			foreach (var file in sourceFiles)
			{
				var tokens = file.FileName.Split('/');
				var typeName = Path.GetFileNameWithoutExtension(tokens[^1]);

				if (string.Compare(typeName, type.Name, false) == 0)
				{
					var cmp = Tenant.GetService<IComponentService>().SelectComponent(file.Component);

					if (cmp != null)
						return cmp;

					break;
				}
			}

			var component = (Guid)typeInfo.GetProperty("Component").GetValue(null);

			return Tenant.GetService<IComponentService>().SelectComponent(component);
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
		private static ConcurrentDictionary<Guid, ManualResetEvent> ScriptCreateState { get { return _scriptCreateState.Value; } }
		private ScriptCache Scripts { get; }

		private static ImmutableArray<DiagnosticAnalyzer> CreateAnalyzers(ITenant tenant, IScriptDescriptor script)
		{
			return new List<DiagnosticAnalyzer>
			{
				new ClassComplianceCodeAnalyer(tenant, script)
			}.ToImmutableArray();
		}

		private void RemoveScript(Guid id)
		{
			Remove(id);
			ScriptContext.RemoveContext(id);
		}
	}
}
