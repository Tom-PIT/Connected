using System;
using TomPIT.ComponentModel;

namespace TomPIT.Ide
{
	internal class Folder : IFolder
	{
		public string Name { get; set; }
		public Guid Token { get; set; }
		public Guid MicroService { get; set; }
		public Guid Parent { get; set; }
	}
}
