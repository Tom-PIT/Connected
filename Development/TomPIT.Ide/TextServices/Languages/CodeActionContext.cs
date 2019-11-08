using System.Collections.Generic;
using Newtonsoft.Json;
using TomPIT.Ide.TextServices.Serialization;

namespace TomPIT.Ide.TextServices.Languages
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
