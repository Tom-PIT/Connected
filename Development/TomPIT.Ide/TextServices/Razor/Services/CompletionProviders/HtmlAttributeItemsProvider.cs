using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Parser.Tokens;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Razor.Services.CompletionProviders.HtmlAttributeCompletionProviders;

namespace TomPIT.Ide.TextServices.Razor.Services.CompletionProviders
{
	internal class HtmlAttributeItemsProvider : CompletionProvider
	{
		private List<ICompletionProvider> _providers = null;

		private List<ICompletionProvider> Providers
		{
			get
			{
				if (_providers == null)
				{
					_providers = new List<ICompletionProvider>
					{
						new CssClassProvider()
					};
				}

				return _providers;
			}
		}

		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();
			var attributeName = ResolveAttributeName();

			if (string.IsNullOrWhiteSpace(attributeName))
				return result;

			foreach (HtmlAttributeProvider provider in Providers)
			{
				provider.AttributeName = attributeName;

				if (!provider.WillProvideItems(Arguments, ImmutableArray.Create(result.ToArray())))
					continue;

				var results = provider.ProvideItems(Arguments);

				if (results != null && results.Count > 0)
					result.AddRange(results);
			}

			return result;
		}

		private string ResolveAttributeName()
		{
			var parser = new HtmlParser(new HtmlParserOptions
			{
				IsKeepingSourceReferences = true,
				IsEmbedded = true,
				IsScripting = true,
				IsStrictMode = false
			});

			var htmlDocument = parser.ParseDocument(Editor.Text);
			var sourceText = SourceText.From(Editor.Text);
			var caret = sourceText.GetCaret(Arguments.Position);

			(IElement element, HtmlAttributeToken attribute) = FindAttributeAtPosition(htmlDocument, caret);

			if (element == null)
				return null;

			var text = GetAttributeText(element, attribute);
			var isValue = IsInValue(caret, attribute, text);

			if (!isValue)
				return null;

			return attribute.Name;
		}

		private bool IsInValue(int caret, HtmlAttributeToken attribute, string attributeText)
		{
			var tokens = attributeText.Trim().Split("=".ToCharArray(), 2);
			var offset = attribute.Position.Index + tokens[0].Length + 1;

			if (caret <= offset)
				return false;

			var relativeCaret = caret - offset;

			if (relativeCaret > tokens[1].Length)
				relativeCaret = tokens[1].Length;

			var value = tokens[1].Substring(0, relativeCaret);
			var doubleQuotes = value.Count(x => x == '"');

			return doubleQuotes == 1;
		}
		private string GetAttributeText(IElement element, HtmlAttributeToken attribute)
		{
			if (!(element.SourceReference is HtmlTagToken tag))
				return element.OuterHtml;

			var text = element.OuterHtml;
			var startIndex = attribute.Position.Index - element.SourceReference.Position.Index - 1;
			var length = text.Length - startIndex;
			HtmlAttributeToken nextAttribute = default;
			var active = false;

			foreach (var att in tag.Attributes)
			{
				if (active)
				{
					nextAttribute = att;
					break;
				}

				if (string.Compare(att.Name, attribute.Name, true) == 0)
					active = true;
			}

			if (!string.IsNullOrEmpty(nextAttribute.Name))
				length = nextAttribute.Position.Index - attribute.Position.Index - 1;

			return text.Substring(startIndex, length);
		}

		private (IElement, HtmlAttributeToken) FindAttributeAtPosition(IHtmlDocument document, int caret)
		{
			foreach (var element in document.All)
			{
				if (!(element.SourceReference is HtmlTagToken tagToken))
					continue;

				if (tagToken.Position.Line != Arguments.Position.LineNumber)
					continue;

				var attribute = AnalyzeElement(element, caret);

				if (attribute.Item1)
					return (element, attribute.Item2);
			}

			return default;
		}

		private (bool, HtmlAttributeToken) AnalyzeElement(IElement element, int caret)
		{
			if (element == null)
				return (false, default);

			if (!(element.SourceReference is HtmlTagToken token))
				return (false, default);

			if (token.Position.Index > caret)
				return AnalyzeElement(element.PreviousElementSibling ?? element.ParentElement, caret);

			var lastFit = new HtmlAttributeToken();
			var hit = false;

			foreach (var attribute in token.Attributes)
			{
				if (attribute.Position.Line != Arguments.Position.LineNumber)
					continue;

				if (attribute.Position.Index > caret)
					break;

				hit = true;
				lastFit = attribute;
			}

			return (hit, lastFit);
		}
	}
}
