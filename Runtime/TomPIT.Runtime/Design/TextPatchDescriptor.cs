using System.Collections.Generic;
using System.Text;

namespace TomPIT.Design
{
	internal class TextPatchDescriptor : ITextPatchDescriptor
	{
		private List<ITextDiffDescriptor> _diffs = null;

		public List<ITextDiffDescriptor> Diffs
		{
			get
			{
				return _diffs ??= new List<ITextDiffDescriptor>();
			}
		}

		public int Start1 { get; set; }
		public int Start2 { get; set; }
		public int Length1 { get; set; }
		public int Length2 { get; set; }

		public override string ToString()
		{
			string coords1;
			string coords2;

			if (Length1 == 0)
				coords1 = $"{Start1},0";
			else if (Length1 == 1)
				coords1 = (Start1 + 1).ToString();
			else
				coords1 = $"{(Start1 + 1)}, {Length1}";
			if (Length2 == 0)
				coords2 = $"{Start2},0";
			else if (Length2 == 1)
				coords2 = (Start2 + 1).ToString();
			else
				coords2 = $"{Start2 + 1}, {Length2}";

			var text = new StringBuilder();

			text.Append("@@ -").Append(coords1).Append(" +").Append(coords2).Append(" @@\n");

			foreach (var diff in Diffs)
			{
				switch (diff.Operation)
				{
					case TextDiffOperation.Insert:
						text.Append('+');
						break;
					case TextDiffOperation.Delete:
						text.Append('-');
						break;
					case TextDiffOperation.Equal:
						text.Append(' ');
						break;
				}

				text.Append(TextDiffProcessor.EncodeURI(diff.Text)).Append("\n");
			}

			return text.ToString();
		}
	}
}