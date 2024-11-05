using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public class PullRequest : IPullRequest
	{
		public Guid Token { get; set; }
		public string Name { get; set; }

		public Guid Template { get; set; }

		public long Branch { get; set; }

		public long Commit { get; set; }

		[JsonConverter(typeof(PullRequestFolderConverter))]
		public List<IPullRequestFolder> Folders { get; set; }
		[JsonConverter(typeof(PullRequestComponentConverter))]
		public List<IPullRequestComponent> Components { get; set; }
	}
}
