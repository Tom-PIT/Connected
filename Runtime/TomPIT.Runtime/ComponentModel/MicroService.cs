using System;
using System.ComponentModel;

namespace TomPIT.ComponentModel
{
	internal class MicroService : IMicroService
	{
		[Browsable(false)]
		public string Name { get; set; }
		[Browsable(false)]
		public string Url { get; set; }
		[Browsable(false)]
		public Guid Token { get; set; }
		[Browsable(false)]
		public MicroServiceStatus Status { get; set; }
		[Browsable(false)]
		public Guid ResourceGroup { get; set; }
		[Browsable(false)]
		public Guid Template { get; set; }
		[Browsable(false)]
		public Guid Package { get; set; }
	}
}
