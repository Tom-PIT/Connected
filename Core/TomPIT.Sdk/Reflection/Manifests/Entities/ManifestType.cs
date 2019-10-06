using System.Collections.Generic;
using TomPIT.Development;

namespace TomPIT.Reflection.Manifests.Entities
{
	public enum ImplementationStatus
	{
		Valid = 1,
		Invalid = 2
	}
	public class ManifestType
	{
		private List<ManifestProperty> _properties = null;
		private List<IDevelopmentError> _diagnostics = null;

		public ImplementationStatus ImplementationStatus { get; set; } = ImplementationStatus.Valid;
		public string ImplementationError { get; set; }
		public string Name { get; set; }
		public string Documentation { get; set; }

		public List<IDevelopmentError> Diagnostics
		{
			get
			{
				if (_diagnostics == null)
					_diagnostics = new List<IDevelopmentError>();

				return _diagnostics;
			}
		}
		public void NotImplemented()
		{
			ImplementationStatus = ImplementationStatus.Invalid;
			ImplementationError = "Not implemented";
		}

		public void SyntaxTreeException()
		{
			ImplementationStatus = ImplementationStatus.Invalid;
			ImplementationError = "SyntaxTree exception";
		}

		public List<ManifestProperty> Properties
		{
			get
			{
				if (_properties == null)
					_properties = new List<ManifestProperty>();

				return _properties;
			}
		}
	}
}
