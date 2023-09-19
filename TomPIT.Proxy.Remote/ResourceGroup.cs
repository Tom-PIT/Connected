using System;
using TomPIT.Environment;

namespace TomPIT.Proxy.Remote
{
	internal class ResourceGroup : IResourceGroup
	{
		public string Name { get; set; }
		public Guid Token { get; set; }
	}
}
