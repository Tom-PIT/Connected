using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public class CodeAction : ICodeAction
	{
		private List<IMarkerData> _diagnostics = null;

		public ICommand Command { get; set; }

		public List<IMarkerData> Diagnostics
		{
			get
			{
				if (_diagnostics == null)
					_diagnostics = new List<IMarkerData>();

				return _diagnostics;
			}
		}

		public IWorkspaceEdit Edit { get; set; }

		public bool IsPreferred { get; set; }

		public string Kind { get; set; }

		public string Title { get; set; }
	}
}
