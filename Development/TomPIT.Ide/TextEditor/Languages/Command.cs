using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public class Command : ICommand
	{
		private List<string> _arguments = null;
		public List<string> Arguments
		{
			get
			{
				if (_arguments == null)
					_arguments = new List<string>();

				return _arguments;
			}
		}

		public string Id { get; set; }

		public string Title { get; set; }

		public string Tooltip { get; set; }
	}
}
