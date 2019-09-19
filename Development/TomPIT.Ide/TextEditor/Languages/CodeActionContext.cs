using System.Collections.Generic;
using Newtonsoft.Json;
using TomPIT.Ide.TextEditor.Serialization;

namespace TomPIT.Ide.TextEditor.Languages
{
	internal class CodeActionContext : ICodeActionContext
	{
		private List<IMarkerData> _markers = null;

		[JsonConverter(typeof(MarkerConverter))]
		public List<IMarkerData> Markers
		{
			get
			{
				if (_markers == null)
					_markers = new List<IMarkerData>();

				return _markers;
			}
		}

		public string Only { get; set; }
	}
}
