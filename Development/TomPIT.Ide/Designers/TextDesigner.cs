using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.Analysis.Analyzers;
using TomPIT.Ide.Analysis.Diagnostics;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Designers.Signatures;
using TomPIT.Ide.Dom;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Ide.Designers
{
	public class TextDesigner : DomDesigner<IDomElement>
	{
		private ITextSignature _signature = null;
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

		public ITextSignature Signature
		{
			get
			{
				if (_signature == null)
				{
					if (string.Compare(Language, "Razor", true) == 0)
						_signature = MethodSignature.CreateModel(typeof(IMiddlewareContext));
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
			if (string.Compare(action, "provideItems", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.Suggestions(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "checkSyntax", true) == 0)
				return CheckSyntax();
			else if (string.Compare(action, "hover", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.Hover(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "signatureHelp", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.Signatures(Environment.Context, CreateSuggestionArgs(data)));
			else if (string.Compare(action, "codeLens", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.CodeLens(Environment.Context, CreateCodeLensArgs(data)));
			else if (string.Compare(action, "resolvePath", true) == 0)
				return Result.JsonResult(this, new JObject
				{
					{"path", Environment.ResolvePath(data.Required<Guid>("component"), data.Required<Guid>("element"), out _) }
				});
			else if (string.Compare(action, "dataSources", true) == 0)
				return Result.JsonResult(this, QueryDataSources());
			else if (string.Compare(action, "definition", true) == 0)
				return Result.JsonResult(this, CodeAnalyzer.Definition(Environment.Context, CreateSuggestionArgs(data)));

			return base.OnAction(data, action);
		}

		private object QueryDataSources()
		{
			var ds = Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(Element.MicroService(), "DataSource").OrderBy(f => f.Name);
			var r = new JArray();

			foreach (var i in ds)
			{
				r.Add(new JObject
				{
					{ "value", i.Token },
					{ "text",i.Name }
				});
			}

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

		private IDesignerActionResult CheckSyntax()
		{
			if (CodeDiagnostic == null)
				return Result.EmptyResult(ViewModel);

			var result = CodeDiagnostic.CheckSyntax(Environment, Content as ISourceCode, ArgumentType);

			return Result.JsonResult(this, result);
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
	}
}
