using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextEditor.CSharp.Services;
using TomPIT.Ide.TextEditor.Services;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextEditor.CSharp
{
	internal class CSharpEditor : Editor
	{
		private static List<MetadataReference> _refs = null;

		private MefHostServices _host = null;
		private Workspace _workspace = null;
		private ProjectInfo _projectInfo = null;
		private CSharpCompilationOptions _options = null;
		private DocumentInfo _docInfo = null;
		private Project _project = null;
		private Document _document = null;

		public CSharpEditor()
		{
			Services.TryAdd(typeof(ISyntaxCheckService), new SyntaxCheckService(this));
			Services.TryAdd(typeof(ICodeActionService), new CodeActionService(this));
		}
		protected MefHostServices Host
		{
			get
			{
				if (_host == null)
					_host = MefHostServices.DefaultHost;

				return _host;
			}
		}

		public override Workspace Workspace
		{
			get
			{
				if (_workspace == null)
					_workspace = new AdhocWorkspace(Host);

				return _workspace;
			}
		}

		public virtual Project Project
		{
			get
			{
				if (_project == null)
					_project = ((AdhocWorkspace)Workspace).AddProject(ProjectInfo);

				return _project;
			}
		}

		public static List<MetadataReference> CombineReferences(List<MetadataReference> additionalReferences)
		{
			additionalReferences.AddRange(References);

			return additionalReferences;
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
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(System.Linq.Enumerable)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IServiceReference)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IMiddlewareContext)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(JObject)).Location)
					};
				}

				return _refs;
			}
		}

		public ProjectInfo ProjectInfo
		{
			get
			{
				if (_projectInfo == null)
				{
					var refs = new List<MetadataReference>();

					if (HostType != null)
					{
						refs.Add(MetadataReference.CreateFromFile(Assembly.GetAssembly(HostType).Location));

						_projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true, hostObjectType: HostType);
					}
					else
						_projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true);

					_projectInfo = _projectInfo.WithMetadataReferences(CombineReferences(refs))
						.WithCompilationOptions(CompilationOptions);
				}

				return _projectInfo;
			}
		}

		private CSharpCompilationOptions CompilationOptions
		{
			get
			{
				if (_options == null)
				{
					var usings = new List<string>();

					if (HostType != null)
						usings.Add(HostType.Namespace);

					_options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
						usings: usings,
						metadataReferenceResolver: new AssemblyResolver(Context.Tenant, Context.MicroService.Token),
						sourceReferenceResolver: new ScriptResolver(Context.Tenant, Context.MicroService.Token));
				}

				return _options;
			}
		}

		public DocumentInfo DocumentInfo
		{
			get
			{
				if (_docInfo == null)
				{
					_docInfo = DocumentInfo.Create(DocumentId.CreateNewId(Project.Id), "Script", sourceCodeKind: SourceCodeKind.Script,
						loader: TextLoader.From(TextAndVersion.Create(SourceText.From(Text), VersionStamp.Create())));
				}

				return _docInfo;
			}
		}

		public Document Document
		{
			get
			{
				if (_document == null)
					_document = ((AdhocWorkspace)Workspace).AddDocument(DocumentInfo);

				return _document;
			}
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			if (_workspace != null)
				_workspace.Dispose();

			_workspace = null;
			_host = null;
		}
	}
}
