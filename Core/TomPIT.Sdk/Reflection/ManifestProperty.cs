﻿using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ManifestProperty : ManifestMember, IManifestProperty
	{
		private List<IManifestAttribute> _attributes = null;
		public bool CanRead { get; set; }
		public bool CanWrite { get; set; }
		
		public List<IManifestAttribute> Attributes => _attributes ??= new List<IManifestAttribute>();

		public bool IsPublic {get;set;}
	}
}