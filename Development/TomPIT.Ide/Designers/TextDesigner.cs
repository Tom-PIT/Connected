using Newtonsoft.Json.Linq;
using System;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Design.Services;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Services;

namespace TomPIT.Designers
{
	public class TextDesigner : DomDesigner<IDomElement>
	{
		private ITextSignature _signature = null;
		private ICodeCompletionProvider _provider = null;
		private ICodeDiagnosticProvider _diagnostic = null;
		private Type _argumentType = null;

		private string _text = null;

		public TextDesigner(IEnvironment environment, IDomElement element) : base(environment, element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Template.cshtml";
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
					_text = Connection.GetService<IComponentService>().SelectText(Environment.Context.MicroService(), Content);

				return _text;
			}
		}

		public string PropertyName => string.IsNullOrWhiteSpace(Environment.Selection.Property) ? "Template" : Environment.Selection.Property;

		public string AttributeName => "Text";

		public ITextSignature Signature
		{
			get
			{
				if (_signature == null)
				{
					if (string.Compare(Language, "Razor", true) == 0)
						_signature = MethodSignature.CreateModel(typeof(IExecutionContext));
					else
					{
						if (Owner.Property != null)
							_signature = MethodSignature.Create(Owner.Property);
						else
							_signature = MethodSignature.Create(Owner.Value);
					}
				}

				return _signature;
			}
		}

		private Type ArgumentType
		{
			get
			{
				if (_argumentType == null)
				{
					if (Owner.Property == null)
						_argumentType = typeof(IExecutionContext);
					else
					{
						var att = Owner.Property.FindAttribute<EventArgumentsAttribute>();

						if (att != null)
						{
							_argumentType = att.Type == null
								 ? Types.GetType(att.TypeName)
								 : att.Type;
						}
					}
				}

				return _argumentType;
			}
		}

		public ICodeCompletionProvider CompletionProvider
		{
			get
			{
				if (_provider == null)
					_provider = Connection.GetService<ICodeCompletionService>().GetProvider(Language);

				return _provider;
			}
		}

		public ICodeDiagnosticProvider CompletionDiagnostic
		{
			get
			{
				if (_diagnostic == null)
					_diagnostic = Connection.GetService<ICodeDiagnosticService>().GetProvider(Language);

				return _diagnostic;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "provideItems", true) == 0)
				return Result.JsonResult(this, CompletionProvider.Suggestions(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "checkSyntax", true) == 0)
				return CheckSyntax();
			else if (string.Compare(action, "hover", true) == 0)
				return Result.JsonResult(this, CompletionProvider.Hover(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "signatureHelp", true) == 0)
				return Result.JsonResult(this, CompletionProvider.Signatures(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "codeLens", true) == 0)
				return Result.JsonResult(this, CompletionProvider.CodeLens(Environment.Context, CreateCodeLensArgs(data)));
			else if (string.Compare(action, "resolvePath", true) == 0)
				return Result.JsonResult(this, new JObject
				{
					{"path", Environment.ResolvePath(data.Required<Guid>("component"), data.Required<Guid>("element")) }
				});

			return base.OnAction(data, action);
		}

		private CodeLensArgs CreateCodeLensArgs(JObject data)
		{
			var component = Connection.GetService<IComponentService>().SelectComponent(Content.Configuration().Component);

			return new CodeLensArgs(component,
				ArgumentType,
				data.Optional("content", string.Empty));
		}

		private CodeStateArgs CreateSuggestionArgs(JObject data)
		{
			var component = Connection.GetService<IComponentService>().SelectComponent(Content.Configuration().Component);

			return new CodeStateArgs(component,
				ArgumentType,
				data.Optional("content", string.Empty),
				data.Optional("position", 0),
				data.Optional("triggerCharacter", string.Empty),
				data.Optional("triggerKind", string.Empty));
		}

		private IDesignerActionResult CheckSyntax()
		{
			var result = CompletionDiagnostic.CheckSyntax(Environment, Content as ISourceCode, ArgumentType);

			return Result.JsonResult(this, result);
		}

		public string DebugFileName
		{
			get
			{
				if (string.Compare(Language, "csharp", true) == 0)
					return string.Format("{0}.csx", Content.Id.ToString());

				return null;
			}
		}
	}
}
