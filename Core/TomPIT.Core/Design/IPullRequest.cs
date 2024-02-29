using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public interface IPullRequest
	{
		Guid Token { get; }
		string Name { get; }
		Guid Template { get; }
		public long Branch { get; set; }
		public long Commit { get; set; }

		List<IPullRequestFolder> Folders { get; }
		List<IPullRequestComponent> Components { get; }
	}
}
