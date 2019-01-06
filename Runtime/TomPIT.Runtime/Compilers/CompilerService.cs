using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Net;
using TomPIT.Runtime;

namespace TomPIT.Compilers
{
	internal class CompilerService : ContextCacheRepository<Script, string>, ICompilerService, ICompilerNotification
	{
		private static string[] Usings = new string[]
		{
				"System",
				"System.Data",
				"System.Text",
				"System.Linq",
				"System.Collections.Generic",
				"Newtonsoft.Json",
				"Newtonsoft.Json.Linq",
				"TomPIT"
		};

		public CompilerService(ISysContext server) : base(server, "script")
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

		private Script GetCachedScript(Guid microService, Guid sourceCodeId)
		{
			return Get(GenerateKey(microService, sourceCodeId));
		}

		public IScriptDescriptor GetScript<T>(Guid microService, ISourceCode sourceCode)
		{
			IScriptDescriptor d = new ScriptDescriptor
			{
				Script = GetCachedScript(microService, sourceCode.Id)
			};

			if (d.Script == null)
				d = CreateScript<T>(microService, sourceCode);

			return d;
		}

		public object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e)
		{
			if (sourceCode.TemplateBlob == Guid.Empty)
				return null;

			var script = GetScript<T>(microService, sourceCode);

			if (script == null)
				return null;

			ScriptGlobals<T> globals = new ScriptGlobals<T>();

			globals.sender = sender;
			globals.e = e;

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
			var severity = ApiSeverity.Critical;

			if (ex.InnerException is ApiException)
				src = ((ApiException)ex.InnerException).Source;

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

					if (i is ApiException)
						severity = ((ApiException)i).Severity;
				}
			}

			var r = new ApiException(src, sb.ToString())
			{
				Severity = severity
			};
			//Log.Error(this, r, EventId.CompileError);

			return r;
		}

		private IScriptDescriptor CreateScript<T>(Guid microService, ISourceCode d)
		{
			var r = new ScriptDescriptor();

			var code = Server.GetService<IComponentService>().SelectTemplate(microService, d);

			if (string.IsNullOrWhiteSpace(code))
				return null;

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
				.WithSourceResolver(new ReferenceResolver(Server, microService))
				.WithMetadataResolver(new MetaDataResolver(Server, microService))
				.WithEmitDebugInformation(true)
				.WithFilePath(string.Format("{0}.csx", d.Id.ToString()))
				.WithFileEncoding(Encoding.UTF8);

			Script<object> script = null;

			using (var loader = new InteractiveAssemblyLoader())
			{
				var refs = ParseReferences(code);

				foreach (var i in refs)
				{
					var asm = MetaDataResolver.LoadDependency(Server, microService, i);

					if (asm != null)
						loader.RegisterDependency(asm);
				}

				script = CSharpScript.Create(code, options: options, globalsType: typeof(ScriptGlobals<T>), assemblyLoader: loader);
			}

			Set(GenerateKey(microService, d.Id), script, TimeSpan.Zero);

			r.Errors = script.Compile();

			//if (r.Errors != null && r.Errors.Length > 0)
			//{
			//	var sb = new StringBuilder();

			//	foreach (var i in r.Errors)
			//		sb.AppendLine(i.ToString());

			//	//Log.Warning(this, sb.ToString(), EventId.CompileError);
			//}

			r.Script = script;

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

		public void Invalidate(IApplicationContext context, Guid microService, Guid component, ISourceCode sourceCode)
		{
			var u = context.GetServerContext().CreateUrl("NotificationDevelopment", "ScriptChanged");

			var args = new JObject
			{
				{ "microService", microService },
				{ "sourceCode", sourceCode.Id }
			};

			Server.Connection.Post(u, args);

			Remove(GenerateKey(microService, sourceCode.Id));
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
			Remove(GenerateKey(e.MicroService, e.SourceCode));
		}
	}
}
