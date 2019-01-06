using System;

namespace TomPIT.Environment
{
	internal class EnvironmentUnit : IEnvironmentUnit
	{
		public string Name { get; set; }
		public Guid Token { get; set; }
		public Guid Parent { get; set; }
		public int Ordinal { get; set; }
	}
}
