using System;

namespace TomPIT.Design
{
	internal class TextDiffArgs : EventArgs
	{
		public string Original { get; set; }
		public string Modified { get; set; }
		public TextDiffCompareMode Mode { get; set; }
		public float Timeout { get; set; }

		public TextDiffArgs Copy()
		{
			return new TextDiffArgs
			{
				Original = Original,
				Modified = Modified,
				Mode = Mode,
				Timeout = Timeout
			};
		}

		public TextDiffArgs WithNewText(string text1, string text2)
		{
			var copy = Copy();

			copy.Original = text1;
			copy.Modified = text2;

			return copy;
		}
	}
}
