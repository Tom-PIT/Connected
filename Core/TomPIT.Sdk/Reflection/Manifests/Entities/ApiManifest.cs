using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection.Manifests.Entities
{
	public class ApiManifest : ComponentManifest
	{
		private List<ApiOperationManifest> _operations = null;
		private List<ManifestMember> _types = null;
		private ApiManifestContact _contact = null;
		private ApiManifestLicense _license = null;

		public ElementScope Scope { get; set; }
		public Version Version { get; set; }

		public ApiManifestLicense License => _license ??= new ApiManifestLicense();
		public ApiManifestContact Contact => _contact ??= new ApiManifestContact();

		public string TermsOfService { get; set; }
		public string Documentation { get; set; }

		public List<ApiOperationManifest> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new List<ApiOperationManifest>();

				return _operations;
			}
		}

		public List<ManifestMember> Types
		{
			get
			{
				if (_types == null)
					_types = new List<ManifestMember>();

				return _types;
			}
		}
	}
}
