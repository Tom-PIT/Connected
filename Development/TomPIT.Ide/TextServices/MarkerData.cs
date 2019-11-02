using System.Collections.Generic;
using Newtonsoft.Json;
using TomPIT.Ide.TextServices.Serialization;

namespace TomPIT.Ide.TextServices
{
	internal class MarkerData : IMarkerData
	{
		private List<IRelatedInformation> _relatedInformation = null;
		public string Code { get; set; }

		public bool External { get; set; } = false;
		public int EndColumn { get; set; }

		public int EndLineNumber { get; set; }

		public string Message { get; set; }

		[JsonConverter(typeof(RelatedInformationConverter))]
		public List<IRelatedInformation> RelatedInformation
		{
			get
			{
				if (_relatedInformation == null)
					_relatedInformation = new List<IRelatedInformation>();

				return _relatedInformation;
			}
		}

		public MarkerSeverity Severity { get; set; }

		public string Source { get; set; }

		public int StartColumn { get; set; }

		public int StartLineNumber { get; set; }

		public List<MarkerTag> Tags { get; set; }
	}
}
