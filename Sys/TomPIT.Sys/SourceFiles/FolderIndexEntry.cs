using System;
using TomPIT.ComponentModel;

namespace TomPIT.Sys.SourceFiles;

public class FolderIndexEntry : IFolder
{
	public string Name { get; set; }
	public Guid Token { get; set; }
	public Guid MicroService { get; set; }
	public Guid Parent { get; set; }

	public static FolderIndexEntry From(IFolder folder)
	{
		return new FolderIndexEntry
		{
			Name = folder.Name,
			Token = folder.Token,
			MicroService = folder.MicroService,
			Parent = folder.Parent
		};
	}
}
