using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public class CompletionList : ICompletionList
	{
		private List<ICompletionItem> _suggestions = null;
		public bool Incomplete { get; set; }

		public List<ICompletionItem> Suggestions
		{
			get
			{
				if (_suggestions == null)
					_suggestions = new List<ICompletionItem>();

				return _suggestions;
			}
		}
	}
}
