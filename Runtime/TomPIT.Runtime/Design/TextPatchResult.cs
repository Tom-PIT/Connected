using System.Collections.Generic;

namespace TomPIT.Design
{
	internal class TextPatchResult : ITextPatchResult
	{
		private List<bool> _patches = null;
		public string Text { get; set; }

		public List<bool> Patches
		{
			get
			{
				return _patches ??= new List<bool>();
			}
		}
	}
}
