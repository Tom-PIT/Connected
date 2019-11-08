using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public class ResourceTextEdit : ResourceEdit, IResourceTextEdit
	{
		private List<ITextEdit> _edits = null;
		public List<ITextEdit> Edits
		{
			get
			{
				if (_edits == null)
					_edits = new List<ITextEdit>();

				return _edits;
			}
		}

		public int ModelVersionId { get; set; }

		public string Resource { get; set; }
	}
}
