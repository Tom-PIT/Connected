using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace TomPIT.Design.CodeAnalysis
{
	public static class CommentsExtensions
	{
		public static string SingleLineComment(this SyntaxToken token, TextSpan span, out int spanStart)
		{
			spanStart = 0;

			if (!token.HasLeadingTrivia)
				return null;

			var trivia = token.LeadingTrivia;

			if (!span.IntersectsWith(trivia.Span))
				return null;

			foreach (var line in trivia)
			{
				if (line.Span.IntersectsWith(span) && line.IsKind(SyntaxKind.SingleLineCommentTrivia))
				{
					var text = line.ToFullString();
					spanStart = line.SpanStart;

					if (text == null)
						return string.Empty;

					var comment = text.Trim();

					if (comment.StartsWith("//"))
					{
						comment = comment.Substring(2);
						spanStart += 2;
					}

					return comment;
				}
			}

			return null;
		}
	}
}
