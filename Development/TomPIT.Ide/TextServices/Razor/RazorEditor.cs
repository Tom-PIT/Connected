using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.Razor.Services;
using TomPIT.Ide.TextServices.Services;
using TomPIT.Middleware;
using TomPIT.Runtime.UI;

namespace TomPIT.Ide.TextServices.Razor
{
	public class RazorEditor : CSharpEditorBase
	{
		private const string ImportUsings = "@using System;@using System.Linq;\r\n@using TomPIT;\r\n@using Microsoft.AspNetCore.Mvc.Rendering;";
		private SourceText _sourceText = null;
		private ProjectInfo _projectInfo = null;
		private DocumentInfo _documentInfo = null;
		public RazorEditor(IMicroServiceContext context) : base(context)
		{
			Services.TryAdd(typeof(ISyntaxCheckService), new SyntaxCheckService(this));
			Services.TryAdd(typeof(ICompletionItemService), new CompletionItemService(this));
			Services.TryAdd(typeof(ICodeActionService), new CodeActionService(this));
			Services.TryAdd(typeof(ISignatureHelpService), new SignatureHelpService(this));
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
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IHtmlContent)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(Renderer)).Location),
						MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(HtmlHelperPartialExtensions)).Location)

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
					_documentInfo = DocumentInfo.Create(DocumentId.CreateNewId(Project.Id), CreateDocumentName(), sourceCodeKind: SourceCodeKind.Regular, isGenerated: true,
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
			| LanguageFeature.CompletionItem
			| LanguageFeature.CodeAction
			| LanguageFeature.SignatureHelp;

		private void CreateSourceText()
		{
			var fileSystem = RazorProjectFileSystem.Create(".");

			var engine = RazorProjectEngine.Create(RazorConfiguration.Default, fileSystem, (builder) =>
			{
				builder.SetBaseType($"TomPIT.Runtime.Design.UI.DesignViewBase");
			});
			
			var document = RazorSourceDocument.Create(Text, Encoding.UTF8, new RazorSourceDocumentProperties($"{Model.Id}.cshtml", $"{Model.Id}.cshtml"));
			var imports = ImmutableArray.Create(RazorSourceDocument.Create(ImportUsings, "ImportUsings.cshtml"));
			var tagHelpers = ImmutableArray.Create<TagHelperDescriptor>();

			var sourceResult = engine.ProcessDesignTime(document, "view", imports, tagHelpers);

			_sourceText = SourceText.From(sourceResult.GetCSharpDocument().GeneratedCode);
		}

		public override int GetMappedCaret(IPosition position)
		{
			var token = GetTokenAtMappedPosition(position);

			if (token == default)
				return -1;

			return SourceText.GetCaret(token.GetLocation().GetLineSpan().StartLinePosition);
		}
		public override int GetMappedCaret(IRange range)
		{
			return GetMappedCaret(new Position
			{
				Column = range.StartColumn,
				LineNumber = range.StartLineNumber
			});
		}
		public override TextSpan GetMappedSpan(IPosition position)
		{
			var token = GetTokenAtMappedPosition(position);

			if (token == default)
				return default;

			return token.GetLocation().SourceSpan;
		}
		public override IPosition GetMappedPosition(IPosition position)
		{
			var token = GetTokenAtMappedPosition(position);

			if (token == default)
				return null;

			var startPosition = token.GetLocation().GetLineSpan().StartLinePosition;

			return new Position
			{
				Column = startPosition.Character,
				LineNumber = startPosition.Line
			};
		}

		private SyntaxToken GetTokenAtMappedPosition(IPosition position)
		{
			var model = Document.GetSemanticModelAsync().Result;

			foreach (var token in model.SyntaxTree.GetRoot().DescendantTokens())
			{
				if (IsOverlapping(token, position))
					return token;
			}

			return default;
		}

		private bool IsOverlapping(SyntaxToken token, IPosition position)
		{
			var mappedSpan = token.GetLocation().GetMappedLineSpan();

			if (!mappedSpan.HasMappedPath)
				return false;

			var path = $"{Model.Id}.cshtml";
			var mappedPath = mappedSpan.Path;

			if (string.Compare(path, mappedPath, true) != 0)
				return false;

			var mappedLine = position.LineNumber;
			var mappedColumn = position.Column;

			var result = mappedSpan.StartLinePosition.Line == mappedLine
				&& (mappedSpan.Span.Start.Character <= mappedColumn && mappedSpan.Span.End.Character >= mappedColumn);

			if (result)
				return result;

			var next = token.GetNextToken();

			if (next == default)
				return result;

			var nextMappedSpan = token.GetLocation().GetMappedLineSpan();

			if (!nextMappedSpan.HasMappedPath)
				return result;

			if (new LinePosition(mappedLine, mappedColumn) < nextMappedSpan.StartLinePosition)
				return true;

			return result;
		}
	}
}
