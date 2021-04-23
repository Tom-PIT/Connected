using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Design
{
	internal class TextPatchResult : ITextPatchResult
	{
		private List<bool> _patches = null;
		public string Text { get; set; }

		public List<bool> Patches => _patches ??= new List<bool>();

		public bool Success => !Patches.Any(f => !f);
	}
}
