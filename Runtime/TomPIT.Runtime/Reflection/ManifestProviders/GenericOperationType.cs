using System.Collections.Generic;

namespace TomPIT.Reflection.ManifestProviders
{
	internal class GenericOperationType : OperationType, IScriptManifestReturnType, IScriptManifestExtenderSupportedType
	{
		private List<string> _extenders;
		public List<string> Extenders => _extenders ??= new List<string>();

		public string ReturnType {get;set;}
	}
}
