using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Compilers;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	public abstract class AnalyzerBase : IDisposable
	{
		private CompletionService _service = null;
		private DocumentInfo _docInfo = null;
		private Document _document = null;
		private CSharpCompilationOptions _options = null;
		private MefHostServices _host = null;
		private AdhocWorkspace _workSpace = null;
		private static List<MetadataReference> _refs = null;
		private ProjectInfo _projectInfo = null;
		private Project _project = null;

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

		public AnalyzerBase(IExecutionContext context)
		{
			Context = context;
		}

		protected IExecutionContext Context { get; }

		public virtual CompletionService Completion
		{
			get
			{
				if (_service == null)
					_service = CompletionService.GetService(Document);

				return _service;
			}
		}

		protected static List<MetadataReference> References
		{
			get
			{
				if (_refs == null)
				{
					_refs = new List<MetadataReference>
					{
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(object)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(Shell)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(StringUtils)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(JObject)).Location)
					};
				}

				return _refs;
			}
		}

		protected virtual CSharpCompilationOptions CompilationOptions
		{
			get
			{
				if (_options == null)
					_options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
				usings: CombineUsings(new List<string> { ArgumentsType.Namespace }),
				metadataReferenceResolver: new MetaDataResolver(Context.Connection(), MicroService),
				sourceReferenceResolver: new ReferenceResolver(Context.Connection(), MicroService));

				return _options;
			}
		}

		protected virtual Type ArgumentsType { get { return typeof(object); } }
		protected virtual Guid MicroService { get { return Context.MicroService(); } }

		public static List<MetadataReference> CombineReferences(List<MetadataReference> additionalReferences)
		{
			additionalReferences.AddRange(References);

			return additionalReferences;
		}

		public void Dispose()
		{
			if (_workSpace != null)
				_workSpace.Dispose();

			_workSpace = null;
			_host = null;
		}

		protected AdhocWorkspace Workspace
		{
			get
			{
				if (_workSpace == null)
					_workSpace = new AdhocWorkspace(Host);

				return _workSpace;
			}
		}

		protected MefHostServices Host
		{
			get
			{
				if (_host == null)
					_host = MefHostServices.Create(MefHostServices.DefaultAssemblies);

				return _host;
			}
		}

		public static string[] CombineUsings(List<string> additionalUsings)
		{
			if (additionalUsings == null || additionalUsings.Count == 0)
				return Usings.ToArray();

			additionalUsings.AddRange(Usings);

			return additionalUsings.ToArray();
		}

		public virtual Project Project
		{
			get
			{
				if (_project == null)
					_project = Workspace.AddProject(ProjectInfo);

				return _project;
			}
		}

		public virtual DocumentInfo DocumentInfo
		{
			get
			{
				if (_docInfo == null)
					_docInfo = DocumentInfo.Create(
				 DocumentId.CreateNewId(Project.Id), "Script",
				 sourceCodeKind: SourceCodeKind.Script,
				 loader: TextLoader.From(TextAndVersion.Create(SourceCode, VersionStamp.Create())));

				return _docInfo;
			}
		}

		public virtual SourceText SourceCode { get { return SourceText.From(string.Empty); } }
		public virtual ProjectInfo ProjectInfo
		{
			get
			{
				if (_projectInfo == null)
				{
					var refs = new List<MetadataReference>
					{
						MetadataReference.CreateFromFile(Assembly.GetAssembly(ArgumentsType).Location)
					};

					_projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true, hostObjectType: ArgumentsType)
					.WithMetadataReferences(CombineReferences(refs))
					.WithCompilationOptions(CompilationOptions);
				}

				return _projectInfo;
			}
		}

		public virtual Document Document
		{
			get
			{
				if (_document == null)
					_document = Workspace.AddDocument(DocumentInfo);

				return _document;
			}
		}

		protected SymbolInfo GetInvocationSymbolInfo(SemanticModel model, ArgumentSyntax syntax)
		{
			var list = GetArgumentList(syntax);

			if (list == null)
				return new SymbolInfo();

			if (!(list.Parent is InvocationExpressionSyntax invoke))
				return new SymbolInfo();

			return model.GetSymbolInfo(invoke);
		}

		protected IMethodSymbol GetMethodSymbol(SemanticModel model, ArgumentSyntax syntax)
		{
			var si = GetInvocationSymbolInfo(model, syntax);

			if (si.Symbol == null && si.CandidateSymbols.Length == 0)
				return null;

			return si.Symbol == null
				? si.CandidateSymbols[0] as IMethodSymbol
				: si.Symbol as IMethodSymbol;
		}

		protected ArgumentListSyntax GetArgumentList(ArgumentSyntax syntax)
		{
			return syntax.Parent as ArgumentListSyntax;
		}

		protected MethodInfo GetMethodInfo(SemanticModel model, ArgumentSyntax syntax)
		{
			var ms = GetMethodSymbol(model, syntax);

			if (ms == null)
				return null;

			if (ms.IsExtensionMethod)
				ms = ms.GetConstructedReducedFrom();

			var declaringTypeName = string.Format(
				"{0}.{1}, {2}",
				ms.ContainingType.ContainingNamespace.ToString(),
				ms.ContainingType.Name,
				ms.ContainingAssembly.Name
			);

			var type = Type.GetType(declaringTypeName);

			if (type == null)
				return null;

			var methodName = ms.Name;

			var methodArgumentTypeNames = new List<string>();

			foreach (var i in ms.Parameters)
			{
				if (i.Type.ContainingNamespace == null || i.Type.ContainingAssembly == null)
					continue;

				methodArgumentTypeNames.Add(string.Format("{0}.{1}, {2}", i.Type.ContainingNamespace.ToString(), i.Type.Name, i.Type.ContainingAssembly.Name));
			}

			var argumentTypes = methodArgumentTypeNames.Select(typeName => Type.GetType(typeName));

			if (argumentTypes.Count() > 0 && argumentTypes.Contains(null))
				return null;

			return Type.GetType(declaringTypeName).GetMethod(methodName, ms.TypeParameters == null ? 0 : ms.TypeParameters.Length, argumentTypes.ToArray());
		}

		protected ICodeAnalysisProvider GetProvider(ParameterInfo parameter)
		{
			var att = parameter.GetCustomAttribute<CodeAnalysisProviderAttribute>();

			if (att == null)
				return null;

			return att.Type == null
				? Type.GetType(att.TypeName).CreateInstance<ICodeAnalysisProvider>(new object[] { Context })
				: att.Type.CreateInstance<ICodeAnalysisProvider>(new object[] { Context });
		}

		protected ParameterInfo GetParameter(MethodInfo method, ArgumentListSyntax list, ArgumentSyntax args)
		{
			int index = list.Arguments.IndexOf(args);

			var pars = method.GetParameters();

			if (index >= pars.Length)
				return null;

			return pars[index];
		}

		protected LiteralExpressionSyntax GetArgumentExpression(ArgumentSyntax syntax)
		{
			return syntax.Expression as LiteralExpressionSyntax;
		}

		protected string ParseExpressionText(ExpressionSyntax expression)
		{
			return expression.ToString().Trim('"');
		}
	}
}
