using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.Analysis.Analyzers;
using TomPIT.Ide.Analysis.Diagnostics;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Ide.TextServices;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Services;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Ide.Designers
{
	public class TextDesigner : DomDesigner<IDomElement>
	{
		private ICodeAnalyzer _analyzer = null;
		private ICodeDiagnosticProvider _diagnostic = null;
		private Type _argumentType = null;

		private string _text = null;

		public TextDesigner(IDomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Text.cshtml";
		public override object ViewModel => this;

		public IText Content => Element.Value as IText;

		public string Language
		{
			get
			{
				var att = Content.GetType().FindAttribute<SyntaxAttribute>();

				if (att == null)
					return "razor";

				return att.Syntax;
			}
		}

		public string Text
		{
			get
			{
				if (_text == null)
					_text = Environment.Context.Tenant.GetService<IComponentService>().SelectText(Element.MicroService(), Content);

				return _text;
			}
		}

		public string PropertyName => string.IsNullOrWhiteSpace(Environment.Selection.Property) ? "Template" : Environment.Selection.Property;

		public string AttributeName => "Text";

		private Type ArgumentType
		{
			get
			{
				if (_argumentType == null)
				{
					if (Owner.Property == null)
					{
						if (Owner.Component == null)
							_argumentType = typeof(IMiddlewareContext);
						else
						{
							var att = Owner.Component.GetType().FindAttribute<EventArgumentsAttribute>();

							if (att != null)
								_argumentType = att.Type ?? TypeExtensions.GetType(att.TypeName);
						}
					}
					else
					{
						var att = Owner.Property.FindAttribute<EventArgumentsAttribute>();

						if (att != null)
							_argumentType = att.Type ?? TypeExtensions.GetType(att.TypeName);
					}
				}

				return _argumentType;
			}
		}

		public ICodeAnalyzer CodeAnalyzer
		{
			get
			{
				if (_analyzer == null)
					_analyzer = Environment.Context.Tenant.GetService<ICodeAnalyzerService>().GetAnalyzer(Language);

				return _analyzer;
			}
		}

		public ICodeDiagnosticProvider CodeDiagnostic
		{
			get
			{
				if (_diagnostic == null)
					_diagnostic = Environment.Context.Tenant.GetService<ICodeDiagnosticService>().GetProvider(Language);

				return _diagnostic;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "provideCompletionItems", true) == 0)
			{
				var model = DeserializeTextModel(data.Optional<JObject>("model", null));
				using var editor = GetTextEditor(model, data.Optional<string>("text", null));

				if (editor == null)
					return Result.JsonResult(this, null);

				var position = DeserializePosition(data.Optional<JObject>("position", null));
				var context = DeserializeCompletionContext(data.Optional<JObject>("context", null));

				return Result.JsonResult(this, editor.GetService<ICompletionItemService>().ProvideCompletionItems(position, context));
			}
			else if (string.Compare(action, "saveText", true) == 0)
				return SaveText(data);
			else if (string.Compare(action, "checkSyntax", true) == 0)
				return CheckSyntax(data);
			else if (string.Compare(action, "hover", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.Hover(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "signatureHelp", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.Signatures(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "codeLens", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.CodeLens(Environment.Context, CreateCodeLensArgs(data)));
			else if (string.Compare(action, "resolvePath", true) == 0)
			{
				var path = Environment.ResolvePath(data.Required<Guid>("component"), data.Required<Guid>("element"), out _);

				return Result.JsonResult(this, new JObject
				{
					{"path",  path}
				});
			}
			else if (string.Compare(action, "definition", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.Definition(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "provideCodeActions", true) == 0)
			{
				var model = DeserializeTextModel(data.Optional<JObject>("model", null));
				using (var editor = GetTextEditor(model, data.Optional<string>("text", null)))
				{

					if (editor == null)
						return Result.JsonResult(this, null);

					var range = DeserializeRange(data.Optional<JObject>("range", null));
					var context = DeserializeCodeActionContext(data.Optional<JObject>("context", null));

					return Result.JsonResult(this, editor.GetService<ICodeActionService>().ProvideCodeActions(range, context));
				}
			}
			else if (string.Compare(action, "provideDeclaration", true) == 0)
			{
				var model = DeserializeTextModel(data.Optional<JObject>("model", null));
				using (var editor = GetTextEditor(model, data.Optional<string>("text", null)))
				{

					if (editor == null)
						return Result.JsonResult(this, null);

					var position = DeserializePosition(data.Optional<JObject>("position", null));

					return Result.JsonResult(this, editor.GetService<IDeclarationProviderService>().ProvideDeclaration(position));
				}
			}

			return base.OnAction(data, action);
		}

		private ITextEditor GetTextEditor(ITextModel model, string text)
		{
			var editor = Environment.Context.Tenant.GetService<ITextService>().GetEditor(new MicroServiceContext(ResolveMicroService(model.Uri), Environment.Context.Tenant.Url), Language);

			if (editor == null)
				return null;

			editor.Text = text;
			editor.Model = model;
			editor.HostType = ArgumentType;

			return editor;
		}

		private ITextModel DeserializeTextModel(JObject data)
		{
			var r = new TextModel();

			Serializer.Populate(data, r);

			return r;
		}

		private TextServices.IRange DeserializeRange(JObject data)
		{
			var r = new TextServices.Range();

			Serializer.Populate(data, r);

			return r;
		}

		private TextServices.IPosition DeserializePosition(JObject data)
		{
			var r = new TextServices.Position();

			Serializer.Populate(data, r);

			return r;
		}

		private ICodeActionContext DeserializeCodeActionContext(JObject data)
		{
			var r = new CodeActionContext();

			Serializer.Populate(data, r);

			return r;
		}

		private ICompletionContext DeserializeCompletionContext(JObject data)
		{
			var r = new CompletionContext();

			Serializer.Populate(data, r);

			return r;
		}

		private CodeLensArgs CreateCodeLensArgs(JObject data)
		{
			var component = Environment.Context.Tenant.GetService<IComponentService>().SelectComponent(Content.Configuration().Component);

			return new CodeLensArgs(component,
				Content,
				ArgumentType,
				data.Optional("content", string.Empty));
		}

		private CodeStateArgs CreateSuggestionArgs(JObject data)
		{
			var component = Environment.Context.Tenant.GetService<IComponentService>().SelectComponent(Content.Configuration().Component);

			return new CodeStateArgs(component,
				Content,
				ArgumentType,
				data.Optional("content", string.Empty),
				data.Optional("position", 0),
				data.Optional("triggerCharacter", string.Empty),
				data.Optional("triggerKind", string.Empty));
		}

		private IDesignerActionResult SaveText(JObject data)
		{
			var sc = ResolveSourceCode(data.Optional<JObject>("model", null));

			if (sc == null)
				return Result.EmptyResult(this);

			Environment.Context.Tenant.GetService<IComponentDevelopmentService>().Update(sc, data.Optional<string>("text", null));
			Environment.Context.Tenant.GetService<ICompilerService>().Invalidate(Environment.Context, sc.Configuration().MicroService(), sc.Configuration().Component, sc);

			var r = Result.SectionResult(ViewModel, EnvironmentSection.Events);

			return r;
		}

		private IDesignerActionResult CheckSyntax(JObject data)
		{
			var model = data.Optional<JObject>("model", null);
			var sc = ResolveSourceCode(model);

			if (sc == null)
				return Result.EmptyResult(this);

			var component = sc.Configuration().Component;
			List<IMarkerData> result = null;

			using (var editor = GetTextEditor(DeserializeTextModel(model), data.Optional<string>("text", null)))
			{
				Environment.Context.Tenant.GetService<IDesignerService>().ClearErrors(component, sc.Id, Development.ErrorCategory.Syntax);

				if (editor != null)
				{

					result = editor.GetService<ISyntaxCheckService>().CheckSyntax(sc);

					if (result != null && result.Count > 0)
					{
						var errors = new List<IDevelopmentError>();

						foreach (var error in result)
						{
							errors.Add(new DevelopmentError
							{
								Category = ErrorCategory.Syntax,
								Code = error.Code,
								Component = component,
								Element = sc.Id,
								Message = error.Message,
								Severity = ResolveSeverity(error.Severity)
							});
						}

						Environment.Context.Tenant.GetService<IDesignerService>().InsertErrors(component, errors);
					}
				}
			}

			if (result == null)
				return Result.EmptyResult(ViewModel);
			else
				return Result.JsonResult(ViewModel, result);
		}

		private DevelopmentSeverity ResolveSeverity(MarkerSeverity severity)
		{
			switch (severity)
			{
				case MarkerSeverity.Hint:
					return DevelopmentSeverity.Info;
				case MarkerSeverity.Info:
					return DevelopmentSeverity.Info;
				case MarkerSeverity.Warning:
					return DevelopmentSeverity.Warning;
				case MarkerSeverity.Error:
					return DevelopmentSeverity.Error;
				default:
					return DevelopmentSeverity.Info;
			}
		}

		public string DebugFileName
		{
			get
			{
				if (string.Compare(Language, "csharp", true) == 0)
					return string.Format("{0}.csx", Content.Id.ToString());
				else if (string.Compare(Language, "razor", true) == 0)
					return string.Format("{0}.cshtml", Content.Id.ToString());

				return null;
			}
		}

		private IMicroService ResolveMicroService(string uri)
		{
			if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri u))
				return default;

			return Environment.Context.Tenant.GetService<IMicroServiceService>().Select(u.Authority);
		}
		private ISourceCode ResolveSourceCode(JObject data)
		{
			var uriString = data.Optional("uri", string.Empty);

			if (string.IsNullOrWhiteSpace(uriString))
				return default;

			if (!Uri.TryCreate(uriString, UriKind.Absolute, out Uri u))
				return default;

			var ms = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(u.Authority);
			var tokens = u.LocalPath.Trim('/').Split('/');
			var category = tokens[0];
			var component = tokens[1];
			var element = Guid.Empty;

			if (tokens.Length > 2)
				element = new Guid(tokens[2]);

			var cmp = Environment.Context.Tenant.GetService<IComponentService>().SelectComponent(ms.Token, category, component);
			var config = Environment.Context.Tenant.GetService<IComponentService>().SelectConfiguration(cmp.Token);

			if (config == null)
				return null;

			if (element == Guid.Empty || cmp.Token == element)
				return config as ISourceCode;

			return config.Find(element) as ISourceCode;
		}

		public LanguageFeature SupportedFeatures(string language, IMicroService microService)
		{
			using (var editor = Environment.Context.Tenant.GetService<ITextService>().GetEditor(new MicroServiceContext(microService, Environment.Context.Tenant.Url), language))
			{

				if (editor == null)
					return LanguageFeature.None;

				return editor.Features;
			}
		}

		public string ParseModelUri(IText text)
		{
			var component = Environment.Context.Tenant.GetService<IComponentService>().SelectComponent(text.Configuration().Component);

			if (text is IConfiguration)
				return $"/{component.Category}/{component.Name}";

			return $"/{component.Category}/{component.Name}/{text.Id}";
		}
	}
}
