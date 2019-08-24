using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.Caching;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Compilation;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.Resources;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Runtime.Compilers;
using TomPIT.Runtime.Compilers.Views;
using TomPIT.Services;
using TomPIT.Storage;
using TomPIT.UI;

namespace TomPIT.Compilers
{
	internal class CompilerService : ClientRepository<IScriptDescriptor, Guid>, ICompilerService, ICompilerNotification
	{
		private static readonly Lazy<ConcurrentDictionary<Guid, List<Guid>>> _references = new Lazy<ConcurrentDictionary<Guid, List<Guid>>>();
		private static Lazy<ConcurrentDictionary<Guid, ManualResetEvent>> _scriptCreateState = new Lazy<ConcurrentDictionary<Guid, ManualResetEvent>>();

		private static readonly string[] Usings = new string[]
		{
				"System",
				"System.Data",
				"System.Text",
				"System.Linq",
				"System.Collections.Generic",
				"Newtonsoft.Json",
				"Newtonsoft.Json.Linq",
				"TomPIT",
				"TomPIT.Data",
				"TomPIT.Services",
				"TomPIT.ComponentModel",
				"TomPIT.ComponentModel.Apis"
		};

		public CompilerService(ISysConnection connection) : base(connection, "script")
		{
			Connection.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			var scripts = All();

			foreach (var i in scripts)
			{
				if (i.MicroService == e.MicroService)
					Remove(i.Id);
			}
		}

		internal static string[] CombineUsings(List<string> additionalUsings)
		{
			if (additionalUsings == null || additionalUsings.Count == 0)
				return Usings.ToArray();

			additionalUsings.AddRange(Usings);

			return additionalUsings.ToArray();
		}

		private IScriptDescriptor GetCachedScript(Guid sourceCodeId)
		{
			return Get(sourceCodeId);
		}

		public IScriptDescriptor GetScript<T>(Guid microService, ISourceCode sourceCode)
		{
			var d = GetCachedScript(sourceCode.Id);

			if (d == null)
			{
				var re = new ManualResetEvent(false);

				if (!ScriptCreateState.TryAdd(sourceCode.Id, re))
				{
					re = ScriptCreateState[sourceCode.Id];

					re.WaitOne();

					d = GetCachedScript(sourceCode.Id);
				}
				else
				{
					d = CreateScript<T>(microService, sourceCode);

					re.Set();

					ScriptCreateState.TryRemove(sourceCode.Id, out _);
				}
			}

			return d;
		}

        public IScriptDescriptor GetScript(Guid microService, ISourceCode sourceCode)
        {
            var d = GetCachedScript(sourceCode.Id);

            if (d == null)
                d = CreateScript(microService, sourceCode);

            return d;
        }

        public object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e)
        {
            return Execute(microService, sourceCode, sender, e, out bool handled);
        }

		public object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e, out bool handled)
		{
			handled = false;

			if (sourceCode.TextBlob == Guid.Empty)
				return null;

			var script = GetScript<T>(microService, sourceCode);

			if (script == null)
				return null;

			handled = true;

			if (script.Errors != null && script.Errors.Where(f => f.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Count() > 0)
			{
				var sb = new StringBuilder();

				foreach (var i in script.Errors)
					sb.AppendLine(i.Message);

				throw new RuntimeException(sb.ToString());
			}

			var globals = new ScriptGlobals<T>
			{
				sender = sender,
				e = e
			};

			try
			{
				return Task.Run(()=>
				{
					return script.Script(globals);
				}).Result;
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

		private IScriptDescriptor CreateScript(Guid microService, ISourceCode d)
		{
			using (var script = new CompilerScript(Connection, microService, d))
			{
				var result = new ScriptDescriptor
				{
					Id = d.Id,
					MicroService = microService
				};

				script.Create();

				Compile(result, script);

				return result;
			}
		}

		private IScriptDescriptor CreateScript<T>(Guid microService, ISourceCode d)
		{
			using (var script = new CompilerGenericScript<T>(Connection, microService, d))
			{
				var result = new ScriptDescriptor
				{
					Id = d.Id,
					MicroService = microService
				};

				script.Create();

				Compile(result, script);

				return result;
			}
		}

		private void Compile(IScriptDescriptor script, CompilerScript compiler)
		{
			var errors = compiler.Script == null ? ImmutableArray<Microsoft.CodeAnalysis.Diagnostic>.Empty : compiler.Script.Compile();
			var diagnostics = new List<IDiagnostic>();

			foreach(var error in errors)
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
				diagnostic.Id = error.Id;
				diagnostics.Add(diagnostic);
			}

			((ScriptDescriptor)script).Errors = diagnostics;

			if (compiler.Script != null && script.Errors.Where(f => f.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Count() == 0)
			{
				((ScriptDescriptor)script).Script = compiler.Script.CreateDelegate();
				((ScriptDescriptor)script).Assembly = compiler.Script.GetCompilation().AssemblyName;
			}

			if (compiler.ScriptReferences != null && compiler.ScriptReferences.Count > 0)
				References.AddOrUpdate(compiler.SourceCode.Id, compiler.ScriptReferences, (key, oldValue) => oldValue = compiler.ScriptReferences);

			Set(script.Id, script, TimeSpan.Zero);
		}

		public void Invalidate(IExecutionContext context, Guid microService, Guid component, ISourceCode sourceCode)
		{
			var u = context.Connection().CreateUrl("NotificationDevelopment", "ScriptChanged");
			var id = sourceCode.ScriptId();

			var args = new JObject
			{
				{ "microService", microService },
				{ "container", component },
				{ "sourceCode", id }
			};

			Connection.Post(u, args);
			Remove(sourceCode.Id);
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
			Remove(e.SourceCode);
			InvalidateReferences(e.Container, e.SourceCode);
		}

		private void InvalidateReferences(Guid container, Guid script)
		{
			foreach (var reference in References)
			{
				if (reference.Value.Contains(container) || reference.Value.Contains(script))
				{
					Remove(reference.Key);
					References.Remove(reference.Key, out _);
				}
			}
		}

		public string CompileView(ISysConnection connection, ISourceCode sourceCode)
		{
			if (sourceCode.TextBlob == Guid.Empty)
				return null;

			var config = sourceCode.Configuration() as IConfiguration;
			var cmp = connection.GetService<IComponentService>().SelectComponent(config.Component);

			if (cmp == null)
				return null;

			var rendererAtt = config.GetType().FindAttribute<ViewRendererAttribute>();
			var content = string.Empty;

			if(rendererAtt != null)
			{
				var renderer = (rendererAtt.Type ?? Types.GetType(rendererAtt.TypeName)).CreateInstance<IViewRenderer>();

				content = renderer.CreateContent(connection, cmp);
			}
			else
			{
				var r = Connection.GetService<IStorageService>().Download(sourceCode.TextBlob);
				
				if (r == null)
					return null;

				content = Encoding.UTF8.GetString(r.Content);
			}

			if (string.IsNullOrWhiteSpace(content))
				return null;

			ProcessorBase processor = null;

			if (sourceCode is ISnippet snippet)
				processor = new SnippetProcessor(snippet, content);
			else
			{
				if (config is IMasterView master)
					processor = new MasterProcessor(master, content);
				else if (config is IView view)
					processor = new ViewProcessor(view, content);
				else if (config is IPartialView partial)
					processor = new PartialProcessor(partial, content);
				else if (config is IMailTemplate mail)
					processor = new MailTemplateProcessor(mail, content);
				else if (config is IReport report)
					processor = new ReportProcessor(report);
			}

			if (processor == null)
				return null;

			processor.Compile(connection, cmp, config as IConfiguration);

			return processor.Result;
		}

		public Type ResolveType(Guid microService, ISourceCode sourceCode, string typeName)
		{
			return ResolveType(microService, sourceCode, typeName, true);
		}
		public Type ResolveType(Guid microService, ISourceCode sourceCode, string typeName, bool throwException)
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
			{
				var sb = new StringBuilder();

				foreach (var error in script.Errors)
					sb.AppendLine(error.Message);

				throw new RuntimeException(typeName, sb.ToString());
			}

			var assembly = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
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

		public IScriptContext CreateScriptContext(ISourceCode sourceCode)
		{
			return new ScriptContext(Connection, sourceCode);
		}

		private static ConcurrentDictionary<Guid, List<Guid>> References { get { return _references.Value; } }
		private static ConcurrentDictionary<Guid, ManualResetEvent> ScriptCreateState { get { return _scriptCreateState.Value; } }
	}
}
