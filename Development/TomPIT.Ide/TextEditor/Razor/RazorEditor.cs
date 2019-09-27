using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Ide.TextEditor.CSharp;
using TomPIT.Ide.TextEditor.Razor.Services;
using TomPIT.Ide.TextEditor.Services;
using TomPIT.Middleware;
using TomPIT.Runtime.UI;

namespace TomPIT.Ide.TextEditor.Razor
{
	public class RazorEditor : CSharpEditorBase
	{
		private SourceText _sourceText = null;
		private ProjectInfo _projectInfo = null;
		private DocumentInfo _documentInfo = null;
		public RazorEditor(IMicroServiceContext context) : base(context)
		{
			Services.TryAdd(typeof(ISyntaxCheckService), new SyntaxCheckService(this));
			Services.TryAdd(typeof(ICompletionItemService), new CompletionItemService(this));
		}

		public override ProjectInfo ProjectInfo
		{
			get
			{
				if (_projectInfo == null)
				{
					var refs = new List<MetadataReference>()
					{
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(object)).Location),
						MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")),
						/*
						 * mac and linux, not needed on windows 
						 */
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(ViewBase<>)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(RazorPage<>)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(RazorCompiledItemAttribute)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IHtmlHelper)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IHtmlContent)).Location)
					};

					_projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "View", "View", LanguageNames.CSharp, isSubmission: false);

					_projectInfo = _projectInfo.WithMetadataReferences(CombineReferences(refs))
						.WithCompilationOptions(CompilationOptions);
				}

				return _projectInfo;
			}
		}

		public override DocumentInfo DocumentInfo
		{
			get
			{
				if (_documentInfo == null)
				{
					_documentInfo = DocumentInfo.Create(DocumentId.CreateNewId(Project.Id), CreateDocumentName(), sourceCodeKind: SourceCodeKind.Regular,
						loader: TextLoader.From(TextAndVersion.Create(SourceText, VersionStamp.Create())));
				}

				return _documentInfo;
			}
		}


		public override SourceText SourceText
		{
			get
			{
				if (_sourceText == null)
					CreateSourceText();

				return _sourceText;
			}
		}

		public override LanguageFeature Features =>
			LanguageFeature.CheckSyntax
			| LanguageFeature.CompletionItem;

		private void CreateSourceText()
		{
			var fileSystem = RazorProjectFileSystem.Create(".");
			var engine = RazorProjectEngine.Create(RazorConfiguration.Default, fileSystem, (builder) =>
			{
				builder.SetBaseType($"TomPIT.Runtime.Design.UI.DesignViewBase");
			});

			var document = RazorSourceDocument.Create(Text, new RazorSourceDocumentProperties($"{Model.Id}.cshtml", $"{Model.Id}.cshtml"));

			var imports = ImmutableArray.Create<RazorSourceDocument>();
			var tagHelpers = ImmutableArray.Create<TagHelperDescriptor>();

			var sourceResult = engine.ProcessDesignTime(document, "markup", imports, tagHelpers);

			_sourceText = SourceText.From(sourceResult.GetCSharpDocument().GeneratedCode);
		}
	}
}
