using System;

namespace TomPIT.Design
{
	internal class ChangeFolder : IChangeFolder
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public Guid Parent { get; set; }
	}
}
