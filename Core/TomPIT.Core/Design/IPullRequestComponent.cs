using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public enum ComponentVerb
	{
		NotModified = 0,
		Add = 1,
		Edit = 2,
		Delete=3
	}
	public interface IPullRequestComponent
	{
		Guid Token { get; }
		Guid MergeToken { get; }
		string Name { get; }
		string Type { get; }
		Guid Folder { get; }
		string Category { get; }
		string Namespace { get; }
		Guid RuntimeConfiguration { get; }
		ComponentVerb Verb { get; set; }

		List<IPullRequestFile> Files { get; }
	}
}
