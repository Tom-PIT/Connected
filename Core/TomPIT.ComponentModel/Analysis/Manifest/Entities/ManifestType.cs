using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Analysis.Manifest.Entities
{
	public enum ImplementationStatus
	{
		Valid = 1,
		Invalid = 2
	}
	internal class ManifestType
	{
		private List<ManifestProperty> _properties = null;

		public ImplementationStatus ImplementationStatus { get; set; } = ImplementationStatus.Valid;
		public string ImplementationError { get; set; }
		public string Name { get; set; }
		public string Documentation { get; set; }


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
