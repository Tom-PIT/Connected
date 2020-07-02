using System;

namespace TomPIT.App.Resources
{
	internal class CompiledBundle
	{
		public string Content { get; set; }
		public Guid MicroService { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
	}
}
