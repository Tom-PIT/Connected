using System;

namespace TomPIT.Reflection
{
	public class ScriptManifestQueryArgs : EventArgs
	{
		public Guid MicroService { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }

		public IScriptManifestSymbolLocation Location { get; set; }
	}
}
