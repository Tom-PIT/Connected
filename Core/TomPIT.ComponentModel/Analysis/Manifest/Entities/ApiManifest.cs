using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Analysis.Manifest.Entities
{
	internal class ApiManifest : ComponentManifest
	{
		private List<ApiOperationManifest> _operations = null;

		public ElementScope Scope { get; set; }

		public List<ApiOperationManifest> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new List<ApiOperationManifest>();

				return _operations;
			}
		}
	}
}
