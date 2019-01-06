namespace TomPIT.Design
{
	internal class CodeAnalysisResult : ICodeAnalysisResult
	{
		public CodeAnalysisResult(string text)
		{
			Text = text;
		}

		public CodeAnalysisResult(string text, string value, string description)
		{
			Text = text;
			Description = description;
			Value = value;
		}

		public string Text { get; set; }
		public string Description { get; set; }
		public string Value { get; set; }
	}
}
