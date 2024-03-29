﻿namespace TomPIT.Ide.TextServices.Languages
{
	public interface IResourceFileEditOptions
	{
		bool Overwrite { get; }
		bool IgnoreIfNotExists { get; }
		bool IgnoreIfExists { get; }
		bool Recursive { get; }
	}
}
