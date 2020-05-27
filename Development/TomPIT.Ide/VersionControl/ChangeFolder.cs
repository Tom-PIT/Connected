using System;

namespace TomPIT.Ide.VersionControl
{
	internal class ChangeFolder : IChangeFolder
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public Guid Parent { get; set; }
	}
}
