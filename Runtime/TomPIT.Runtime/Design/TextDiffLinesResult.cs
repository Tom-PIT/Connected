using System.Collections.Generic;

namespace TomPIT.Design
{
	internal class TextDiffLinesResult
	{
		private List<string> _lines = null;
		public string Text1 { get; set; }
		public string Text2 { get; set; }

		public List<string> Lines
		{
			get
			{
				return _lines ??= new List<string>();
			}
		}
	}
}
