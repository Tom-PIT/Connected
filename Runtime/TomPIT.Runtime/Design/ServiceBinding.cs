using System;
using TomPIT.Development;

namespace TomPIT.Design
{
	internal class ServiceBinding : IServiceBinding
	{
		public Guid Service { get; set; }

		public long Commit { get; set; }

		public DateTime Date { get; set; }

		public bool Active { get; set; }

		public string RepositoryName { get; set; }

		public string RepositoryUrl { get; set; }

		public DateTime LastCommit { get; set; }

		public string ServiceName { get; set; }
	}
}
