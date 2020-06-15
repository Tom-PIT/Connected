using System;

namespace TomPIT.Data
{
	internal class OperationState
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool Initialized { get; set; }
		public bool Valid { get; set; }
		public string Text { get; set; }
	}
}
