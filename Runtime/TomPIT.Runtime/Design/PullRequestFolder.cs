using System;

namespace TomPIT.Design
{
	public class PullRequestFolder : IPullRequestFolder
	{
		public string Name {get;set;}

		public Guid Id {get;set;}

		public Guid Parent {get;set;}
	}
}
