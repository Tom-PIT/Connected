using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.Caching;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Compilers
{
	internal class CompilerService : ClientRepository<Script, Guid>, ICompilerService, ICompilerNotification
	{
		private Lazy<ConcurrentDictionary<Guid, List<Guid>>> _forwardReferences = new Lazy<ConcurrentDictionary<Guid, List<Guid>>>();
		private Lazy<ConcurrentDictionary<Guid, List<Guid>>> _reverseReferences = new Lazy<ConcurrentDictionary<Guid, List<Guid>>>();

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

		}

		private string[] CombineUsings(List<string> additionalUsings)
		{
			if (additionalUsings == null || additionalUsings.Count == 0)
				return Usings.ToArray();

			additionalUsings.AddRange(Usings);

			return additionalUsings.ToArray();
		}

		public bool Equals(string constant, object value)
		{
			if (string.IsNullOrWhiteSpace(constant))
			{
				return false;
			}

			if (value == null || value == DBNull.Value)
			{
				return false;
			}

			var converter = TypeDescriptor.GetConverter(value);

			string sr = converter.ConvertToInvariantString(value);

			return string.Compare(sr, constant, true) == 0;
		}

		private Script GetCachedScript(Guid sourceCodeId)
		{
			return Get(sourceCodeId);
		}

		public IScriptDescriptor GetScript<T>(Guid microService, ISourceCode sourceCode)
		{
			IScriptDescriptor d = new ScriptDescriptor
			{
				Script = GetCachedScript(sourceCode.Id)
			};

			if (d.Script == null)
				d = CreateScript<T>(microService, sourceCode);

			return d;
		}

		public object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e)
		{
			if (sourceCode.TextBlob == Guid.Empty)
				return null;

			var script = GetScript<T>(microService, sourceCode);

			if (script == null)
				return null;

			var globals = new ScriptGlobals<T>
			{
				sender = sender,
				e = e
			};

			try
			{
				var state = Task.Run(async () =>
				{
					return await script.Script.RunAsync(globals).ConfigureAwait(false);
				}).Result;

				return state.ReturnValue;
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
			}

			var r = new RuntimeException(src, sb.ToString())
			{
				Severity = severity
			};
			//Log.Error(this, r, EventId.CompileError);

			return r;
		}

		private IScriptDescriptor CreateScript<T>(Guid microService, ISourceCode d)
		{
			var r = new ScriptDescriptor();

			var code = Connection.GetService<IComponentService>().SelectText(microService, d);

			if (d is IPartialSourceCode ps)
			{
				var container = d.Closest<ISourceCodeContainer>();

				if (container != null)
				{
					var refs = container.References(ps);

					if (refs != null)
					{
						var sb = new StringBuilder();

						foreach (var i in refs)
						{
							var txt = container.GetReference(i);
							var sc = Connection.GetService<IComponentService>().SelectText(microService, txt);

							if (!string.IsNullOrWhiteSpace(sc))
							{
								sb.AppendLine();
								sb.Append(sc);
							}
						}

						if (sb.Length > 0)
							code = string.Format("{0}{1}", code, sb.ToString());
					}
				}
			}

			if (string.IsNullOrWhiteSpace(code))
				return null;

			ResolveReferences(microService, d, code);
			var imports = CombineUsings(new List<string> { typeof(T).Namespace });

			var references = new List<Assembly>
			{
				typeof(T).Assembly,
				LoadSystemAssembly("TomPIT.Core"),
				LoadSystemAssembly("TomPIT.ComponentModel"),
				LoadSystemAssembly("Newtonsoft.Json")
			};

			var options = ScriptOptions.Default
				.WithImports(imports)
				.WithReferences(references)
				.WithSourceResolver(new ReferenceResolver(Connection, microService))
				.WithMetadataResolver(new MetaDataResolver(Connection, microService))
				.WithEmitDebugInformation(true)
				.WithFilePath(d.ScriptName(Connection))
				.WithFileEncoding(Encoding.UTF8);

			Script<object> script = null;

			using (var loader = new InteractiveAssemblyLoader())
			{
				var refs = ParseReferences(code);

				foreach (var i in refs)
				{
					var asm = MetaDataResolver.LoadDependency(Connection, microService, i);

					if (asm != null)
						loader.RegisterDependency(asm);
				}

				script = CSharpScript.Create(code, options: options, globalsType: typeof(ScriptGlobals<T>), assemblyLoader: loader);
			}

			Set(d.Id, script, TimeSpan.Zero);

			r.Errors = script.Compile();
			r.Script = script;

			return r;
		}

		private void ResolveReferences(Guid microService, ISourceCode d, string code)
		{
			var scriptId = d.ScriptId();
			var scripts = ParseScripts(code);
			var ids = new List<Guid>();

			foreach (var i in scripts)
			{
				var path = Path.GetFileNameWithoutExtension(i);

				var lib = Connection.GetService<IComponentService>().SelectComponent(microService, "Library", path);

				if (lib == null)
					continue;

				if (!ids.Contains(lib.Token))
					ids.Add(lib.Token);

				List<Guid> forward = null;

				if (ForwardReferences.ContainsKey(lib.Token))
					forward = ForwardReferences[lib.Token];
				else
				{
					forward = new List<Guid>();

					if (!ForwardReferences.TryAdd(lib.Token, forward))
						ForwardReferences.TryGetValue(lib.Token, out forward);
				}

				if (!forward.Contains(scriptId))
				{
					lock (forward)
					{
						forward.Add(scriptId);
					}
				}

				List<Guid> reverse = null;

				if (ReverseReferences.ContainsKey(scriptId))
					reverse = ReverseReferences[scriptId];
				else
				{
					reverse = new List<Guid>();

					if (!ReverseReferences.TryAdd(scriptId, reverse))
						ReverseReferences.TryGetValue(scriptId, out reverse);
				}

				if (!reverse.Contains(lib.Token))
				{
					lock (reverse)
					{
						reverse.Add(lib.Token);
					}
				}
			}

			if (!ReverseReferences.ContainsKey(scriptId))
				return;

			var cleanup = ReverseReferences[scriptId];
			var obsolete = new List<Guid>();

			foreach (var i in cleanup)
			{
				if (!ids.Contains(i))
				{
					obsolete.Add(i);

					if (!ForwardReferences.ContainsKey(i))
						continue;

					var refs = ForwardReferences[i];

					if (refs.Contains(scriptId))
					{
						lock (refs)
						{
							refs.Remove(scriptId);
						}
					}
				}
			}

			if (obsolete.Count > 0)
			{
				lock (cleanup)
				{
					foreach (var i in obsolete)
					{
						cleanup.Remove(i);
					}
				}
			}
		}

		private List<string> ParseScripts(string code)
		{
			var refs = Regex.Matches(code, "#load.*");
			var r = new List<string>();

			foreach (Match i in refs)
			{
				var name = Regex.Match(i.Value, "\"([^\"]*)\"");

				if (name == null)
					continue;

				r.Add(name.Value.Trim('"'));
			}

			return r;
		}

		private List<string> ParseReferences(string code)
		{
			var refs = Regex.Matches(code, "^#r.*");
			var r = new List<string>();

			foreach (Match i in refs)
			{
				var name = Regex.Match(i.Value, "\"([^\"]*)\"");

				if (name == null)
					continue;

				r.Add(name.Value.Trim('"'));
			}

			return r;
		}

		public void Invalidate(IExecutionContext context, Guid microService, Guid component, ISourceCode sourceCode)
		{
			var u = context.Connection().CreateUrl("NotificationDevelopment", "ScriptChanged");
			var id = sourceCode.ScriptId();

			var args = new JObject
			{
				{ "microService", microService },
				{ "sourceCode", id }
			};

			Connection.Post(u, args);
			Remove(sourceCode.Id);
			InvalidateReferences(id);
		}

		private Assembly LoadSystemAssembly(string fileName)
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
			InvalidateReferences(e.SourceCode);
		}

		private void InvalidateReferences(Guid id)
		{
			if (ForwardReferences.ContainsKey(id))
			{
				var refs = ForwardReferences[id];

				foreach (var i in refs)
					Remove(i);
			}
		}

		private ConcurrentDictionary<Guid, List<Guid>> ForwardReferences { get { return _forwardReferences.Value; } }
		private ConcurrentDictionary<Guid, List<Guid>> ReverseReferences { get { return _reverseReferences.Value; } }
	}
}
