using System;
using TomPIT.Development;

namespace TomPIT.Design
{
	internal class ComponentHistory : IComponentHistory
	{
		public DateTime Created { get; set; }
		public Guid Blob { get; set; }
		public string Name { get; set; }
		public Guid User { get; set; }
		public Guid Commit { get; set; }
		public Guid Component { get; set; }
		public LockVerb Verb { get; set; }
	}
}
