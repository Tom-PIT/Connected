using System;

namespace TomPIT.ComponentModel
{
	internal class MicroService : IMicroService
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public Guid Token { get; set; }
		public MicroServiceStatus Status { get; set; }
		public Guid ResourceGroup { get; set; }
		public Guid Template { get; set; }
		public Guid Package { get; set; }
	}
}
