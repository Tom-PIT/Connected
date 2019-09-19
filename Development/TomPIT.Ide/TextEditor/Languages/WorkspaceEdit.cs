using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public class WorkspaceEdit : IWorkspaceEdit
	{
		private List<IResourceEdit> _edits = null;

		public List<IResourceEdit> Edits
		{
			get
			{
				if (_edits == null)
					_edits = new List<IResourceEdit>();

				return _edits;
			}
		}
	}
}
