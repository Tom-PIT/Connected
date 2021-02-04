using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public enum ComponentVerb
	{
		Add = 1,
		Edit = 2,
		Delete=3
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
