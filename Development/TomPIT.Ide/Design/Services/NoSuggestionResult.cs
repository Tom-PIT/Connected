﻿namespace TomPIT.Design.Services
{
	public class NoSuggestionResult : ICodeAnalysisResult
	{
		public NoSuggestionResult(string text)
		{
			Text = text;
		}

		public string Text { get; }
		public string Description => string.Empty;
		public string Value => string.Empty;
	}
}