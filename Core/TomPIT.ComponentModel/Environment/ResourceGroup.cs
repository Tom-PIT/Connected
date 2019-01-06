using System;

namespace TomPIT.Environment
{
	internal class ResourceGroup : IResourceGroup
	{
		public string Name { get; set; }
		public Guid Token { get; set; }
	}
}
