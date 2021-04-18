using System;
using System.Collections.Generic;
using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection.Api
{
	public class ApiOperationManifest: ManifestMiddleware
	{
		private List<string> _extenders = null;
		public ElementScope Scope { get; set; }
		public bool Distributed { get; set; }
		public HttpVerbs Verbs { get; set; }
		public Guid Id { get; set; }
		public string ReturnType { get; set; }
		public List<string> Extenders
		{
			get
			{
				if (_extenders == null)
					_extenders = new List<string>();

				return _extenders;
			}
		}
	}
}
