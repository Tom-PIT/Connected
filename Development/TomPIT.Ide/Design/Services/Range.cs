using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	internal class Range : IRange
	{
		[JsonProperty(PropertyName = "endColumn")]
		public int EndColumn { get; set; }
		[JsonProperty(PropertyName = "endLineNumber")]
		public int EndLineNumber { get; set; }
		[JsonProperty(PropertyName = "startColumn")]
		public int StartColumn { get; set; }
		[JsonProperty(PropertyName = "startLineNumber")]
		public int StartLineNumber { get; set; }

		public static implicit operator Range(SyntaxNode node)
		{
			if (node == null)
				return null;

			var span = node.GetLocation().GetLineSpan();

			return new Range
			{
				EndColumn = span.EndLinePosition.Character,
				EndLineNumber = span.EndLinePosition.Line + 1,
				StartColumn = span.StartLinePosition.Character,
				StartLineNumber = span.StartLinePosition.Line + 1
			};
		}
	}
}
