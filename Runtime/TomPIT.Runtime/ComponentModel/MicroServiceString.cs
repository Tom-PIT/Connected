using System;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceString : IMicroServiceString
	{
		public Guid MicroService { get; set; }
		public Guid Element { get; set; }
		public string Property { get; set; }
		public string Value { get; set; }
		public Guid Language { get; set; }
	}
}
