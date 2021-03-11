using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextServices.CSharp
{
	public abstract class CSharpEditorBase : Editor
	{
		private static List<MetadataReference> _refs = null;

		private MefHostServices _host = null;
		private Workspace _workspace = null;
		private ProjectInfo _projectInfo = null;
		private CSharpCompilationOptions _options = null;
		private DocumentInfo _docInfo = null;
		private Project _project = null;
		private Document _document = null;
		private SourceText _sourceText = null;
		private SemanticModel _model = null;
		private bool _modelCalled = false;

		public CSharpEditorBase(IMicroServiceContext context) : base(context)
		{
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
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(ValidationResult)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IServiceReference)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IMiddlewareContext)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(JObject)).Location)
					};
				}

				return _refs;
			}
		}

		public virtual ProjectInfo ProjectInfo
		{
			get
			{
				if (_projectInfo == null)
				{
					var refs = new List<MetadataReference>();

					if (HostType != null)
					{
						refs.Add(MetadataReference.CreateFromFile(Assembly.GetAssembly(HostType).Location));

						_projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script",
							LanguageNames.CSharp, isSubmission: true, hostObjectType: typeof(ScriptGlobals<>).MakeGenericType(HostType));
					}
					else
						_projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true);

					_projectInfo = _projectInfo.WithMetadataReferences(CombineReferences(refs))
						.WithCompilationOptions(CompilationOptions);
				}

				return _projectInfo;
			}
		}

		protected CSharpCompilationOptions CompilationOptions
		{
			get
			{
				if (_options == null)
				{
					var usings = new List<string>();

					if (HostType != null)
						usings.Add(HostType.Namespace);

					var included = IncludedUsings();

					if (included != null && included.Count > 0)
						usings.AddRange(included);

					_options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
						usings: usings,
						metadataReferenceResolver: new AssemblyResolver(Context.Tenant, Context.MicroService.Token, true),
						sourceReferenceResolver: new ScriptResolver(Context.Tenant, Context.MicroService.Token));
				}

				return _options;
			}
		}

		protected virtual List<string> IncludedUsings()
		{
			return null;
		}

		public virtual DocumentInfo DocumentInfo
		{
			get
			{
				if (_docInfo == null)
				{
					_docInfo = DocumentInfo.Create(DocumentId.CreateNewId(Project.Id), CreateDocumentName(), sourceCodeKind: SourceCodeKind.Script,
						loader: TextLoader.From(TextAndVersion.Create(SourceText, VersionStamp.Create())));
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

		public virtual SourceText SourceText
		{
			get
			{
				if (_sourceText == null)
					_sourceText = SourceText.From(Text);

				return _sourceText;
			}
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			if (_workspace != null)
				_workspace.Dispose();

			_workspace = null;
			_host = null;

			GC.Collect();
		}

		protected virtual string CreateDocumentName()
		{
			if (Model == null || string.IsNullOrWhiteSpace(Model.Uri))
				return "Script";

			var tokens = Model.Uri.Split('/');

			return tokens[^1];
		}

		public override int GetCaret(IPosition position)
		{
			return SourceText.GetCaret(position);
		}

		public override IPosition GetMappedPosition(IPosition position)
		{
			return position;
		}

		public override int GetMappedCaret(IPosition position)
		{
			return GetCaret(position);
		}

		public override int GetMappedCaret(IRange range)
		{
			return GetCaret(new Position
			{
				Column = range.StartColumn,
				LineNumber = range.StartLineNumber
			});
		}

		public override TextSpan GetMappedSpan(IPosition position)
		{
			return SourceText.GetSpan(position);
		}

		public SemanticModel SemanticModel
		{
			get
			{
				if (_model == null && !_modelCalled)
				{
					_modelCalled = true;

					try
					{
						_model = Document.GetSemanticModelAsync().Result;
					}
					catch { }
				}

				return _model;
			}
		}
	}
}
