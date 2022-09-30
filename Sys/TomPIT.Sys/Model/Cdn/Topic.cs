using System;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	public class Topic : ITopic
	{
		public string Name { get; set; }

		public Guid ResourceGroup { get; set; }

		public long Id { get; set; }
	}
}
