using System;
using System.ComponentModel;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy.Remote
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
		[Browsable(false)]
		public UpdateStatus UpdateStatus { get; set; }
		[Browsable(false)]
		public CommitStatus CommitStatus { get; set; }
		[Browsable(false)]
		public string Version { get; set; }
		[Browsable(false)]
		public Guid Plan { get; set; }
	}
}