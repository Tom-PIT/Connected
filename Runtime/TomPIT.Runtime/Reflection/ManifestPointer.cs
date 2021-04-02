using System;

namespace TomPIT.Reflection
{
	internal struct ManifestPointer : IManifestPointer
	{
		public short Id {get;set;}

		public Guid MicroService {get;set;}

		public Guid Component {get;set;}

		public Guid Element {get;set;}
	}
}
