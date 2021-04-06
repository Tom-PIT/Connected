using System;

namespace TomPIT.Reflection
{
	internal struct ScriptManifestPointer : IScriptManifestPointer
	{
		public short Id {get;set;}

		public Guid MicroService {get;set;}

		public Guid Component {get;set;}

		public Guid Element {get;set;}

		public bool Equals(IScriptManifestPointer other)
		{
			if (other is null)
				return false;

			return MicroService == other.MicroService && Component == other.Component && Element == other.Element;
		}
	}
}
