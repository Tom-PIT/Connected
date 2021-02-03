using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public enum ComponentVerb
	{
		Modify = 1,
		Remove=2
	}
	public interface IPullRequestComponent
	{
		Guid Token { get; }
		string Name { get; }
		string Type { get; }
		Guid Folder { get; }
		string Category { get; }
		string Namespace { get; }
		Guid RuntimeConfiguration { get; }
		ComponentVerb Verb { get; }

		List<IPullRequestFile> Files { get; }
	}
}
