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
				"TomPIT",
				"TomPIT.Data",
				"TomPIT.Services",
				"TomPIT.ComponentModel",
				"TomPIT.ComponentModel.Apis"
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
				{
					var usings = new List<string>();

					if (ArgumentsType != null)
						usings.Add(ArgumentsType.Namespace);

					_options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
						usings: CombineUsings(usings),
						metadataReferenceResolver: new MetaDataResolver(Context.Connection(), MicroService),
						sourceReferenceResolver: new ReferenceResolver(Context.Connection(), MicroService));
				}

				return _options;
			}
		}

		protected virtual Type ArgumentsType { get { return typeof(object); } }
		protected virtual Guid MicroService { get { return Context.MicroService.Token; } }

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
				 sourceCodeKind: SourceCodeKind,
				 loader: TextLoader.From(TextAndVersion.Create(SourceCode, VersionStamp.Create())));

				return _docInfo;
			}
		}

		protected virtual SourceCodeKind SourceCodeKind { get { return SourceCodeKind.Script; } }
		protected virtual bool IsSubmission { get { return true; } }

		public virtual SourceText SourceCode { get { return SourceText.From(string.Empty); } }
		protected virtual string AssemblyName => "Script";
		public virtual ProjectInfo ProjectInfo
		{
			get
			{
				if (_projectInfo == null)
				{
					var refs = new List<MetadataReference>();

					if (ArgumentsType != null)
					{
						refs.Add(MetadataReference.CreateFromFile(Assembly.GetAssembly(ArgumentsType).Location));

						_projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), AssemblyName, AssemblyName, LanguageNames.CSharp, isSubmission: IsSubmission, hostObjectType: ArgumentsType);
					}
					else
						_projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), AssemblyName, AssemblyName, LanguageNames.CSharp, isSubmission: IsSubmission);

					_projectInfo = _projectInfo.WithMetadataReferences(CombineReferences(refs))
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

		protected SymbolInfo GetInvocationSymbolInfo(SemanticModel model, AttributeArgumentListSyntax syntax)
		{
			if (syntax == null)
				return new SymbolInfo();

			if (!(syntax.Parent is AttributeSyntax invoke))
				return new SymbolInfo();

			return model.GetSymbolInfo(invoke);
		}

		protected SymbolInfo GetInvocationSymbolInfo(SemanticModel model, ArgumentListSyntax syntax)
		{
			if (syntax == null)
				return new SymbolInfo();

			if (syntax.Parent is InvocationExpressionSyntax invoke)
				return model.GetSymbolInfo(invoke);

			if((syntax.Parent is ConstructorInitializerSyntax cinvoke))
				return model.GetSymbolInfo(cinvoke);

			return new SymbolInfo();
		}

		protected SymbolInfo GetInvocationSymbolInfo(SemanticModel model, ArgumentSyntax syntax)
		{
			return GetInvocationSymbolInfo(model, syntax.Parent as ArgumentListSyntax);
		}

		protected IMethodSymbol GetMethodSymbol(SemanticModel model, AttributeArgumentListSyntax syntax)
		{
			var si = GetInvocationSymbolInfo(model, syntax);

			if (si.Symbol == null && si.CandidateSymbols.Length == 0)
				return null;

			return si.Symbol == null
				? si.CandidateSymbols[0] as IMethodSymbol
				: si.Symbol as IMethodSymbol;
		}

		protected IMethodSymbol GetMethodSymbol(SemanticModel model, ArgumentListSyntax syntax)
		{
			var si = GetInvocationSymbolInfo(model, syntax);

			if (si.Symbol == null && si.CandidateSymbols.Length == 0)
				return null;

			return si.Symbol == null
				? si.CandidateSymbols[0] as IMethodSymbol
				: si.Symbol as IMethodSymbol;
		}

		protected IMethodSymbol GetMethodSymbol(SemanticModel model, ArgumentSyntax syntax)
		{
			return GetMethodSymbol(model, syntax.Parent as ArgumentListSyntax);
		}

		protected ArgumentListSyntax GetArgumentList(ArgumentSyntax syntax)
		{
			return syntax.Parent as ArgumentListSyntax;
		}

		protected AttributeArgumentListSyntax GetArgumentList(AttributeArgumentSyntax syntax)
		{
			return syntax.Parent as AttributeArgumentListSyntax;
		}

		protected MethodInfo GetMethodInfo(SemanticModel model, AttributeArgumentListSyntax syntax)
		{
			return GetMethodInfo(model, GetMethodSymbol(model, syntax));
		}

		protected MethodInfo GetMethodInfo(SemanticModel model, ArgumentListSyntax syntax)
		{
			return GetMethodInfo(model, GetMethodSymbol(model, syntax));
		}

		protected ConstructorInfo GetConstructorInfo(SemanticModel model, IMethodSymbol ms)
		{
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

			var constructors = Type.GetType(declaringTypeName).GetConstructors();

			foreach (var ctor in constructors)
			{
				if (ctor.GetParameters().Count() != ms.Parameters.Length)
					continue;

				bool match = true;
				var parameters = ctor.GetParameters();

				for (var i = 0; i < methodArgumentTypeNames.Count; i++)
				{
					var at = Types.GetType(methodArgumentTypeNames[i]);

					if (at == null)
						continue;

					var pt = parameters[i].ParameterType;

					if (pt.IsGenericMethodParameter)
						continue;

					if (pt.IsInterface)
					{
						if (at != pt && !at.ImplementsInterface(pt))
						{
							match = false;
							break;
						}
					}
					else if (at != pt && !at.IsSubclassOf(pt))
					{
						match = false;
						break;
					}
				}

				if (match)
					return ctor;
			}

			return null;
		}
		protected MethodInfo GetMethodInfo(SemanticModel model, IMethodSymbol ms)
		{
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
			var methods = Type.GetType(declaringTypeName).GetMethods();

			foreach (var method in methods)
			{
				if (string.Compare(method.Name, ms.Name, false) != 0)
					continue;

				if (ms.TypeParameters != null && method.GetGenericArguments().Count() != ms.TypeParameters.Length)
					continue;

				if (method.GetParameters().Count() != ms.Parameters.Length)
					continue;

				bool match = true;
				var parameters = method.GetParameters();

				for (var i = 0; i < methodArgumentTypeNames.Count; i++)
				{
					var at = Types.GetType(methodArgumentTypeNames[i]);

					if (at == null)
						continue;

					var pt = parameters[i].ParameterType;

					if (pt.IsGenericMethodParameter)
						continue;

					if (pt.IsInterface)
					{
						if (at != pt && !at.ImplementsInterface(pt))
						{
							match = false;
							break;
						}
					}
					else if (at != pt && !at.IsSubclassOf(pt))
					{
						match = false;
						break;
					}
				}

				if (match)
					return method;
			}

			return null;
		}

		protected MethodInfo GetMethodInfo(SemanticModel model, ArgumentSyntax syntax)
		{
			return GetMethodInfo(model, GetArgumentList(syntax));
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

		protected IdentifierNameSyntax GetIdentiferName(InvocationExpressionSyntax syntax)
		{
			if (syntax == null)
				return null;

			var current = syntax.Expression;

			while (current != null)
			{
				if (current is IdentifierNameSyntax idn)
					return idn;

				if (current is MemberAccessExpressionSyntax ma)
					current = ma.Expression;
				else
					break;
			}

			return null;
		}
	}
}
