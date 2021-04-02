using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection.Api
{
	public class ApiManifest : ComponentManifestMiddleware
	{
		private List<ApiOperationManifest> _operations = null;
		private ApiManifestContact _contact = null;
		private ApiManifestLicense _license = null;

		public ApiManifest(IComponentManifestProvider provider) : base(provider)
		{

		}
		public ElementScope Scope { get; set; }
		public Version Version { get; set; }

		public ApiManifestLicense License => _license ??= new ApiManifestLicense();
		public ApiManifestContact Contact => _contact ??= new ApiManifestContact();

		public string TermsOfService { get; set; }
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
