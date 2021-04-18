namespace TomPIT.Design
{
	internal class TextDiffDescriptor : ITextDiffDescriptor
	{
		public TextDiffDescriptor(TextDiffOperation operation, string text)
		{
			Operation = operation;
			Text = text;
		}

		public TextDiffOperation Operation { get; set; }
		public string Text { get; set; }

		public override string ToString()
		{
			var prettyText = Text.Replace('\n', '\u00b6');

			return $"Diff({Operation},\"{prettyText}\")";
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (!(obj is ITextDiffDescriptor p))
				return false;

			return p.Operation == Operation && string.Compare(p.Text, Text, false) == 0;
		}

		public bool Equals(ITextDiffDescriptor obj)
		{
			if (obj == null)
				return false;

			return obj.Operation == Operation && string.Compare(obj.Text, Text, false) == 0;
		}

		public override int GetHashCode()
		{
			return Text.GetHashCode() ^ Operation.GetHashCode();
		}
	}
}
