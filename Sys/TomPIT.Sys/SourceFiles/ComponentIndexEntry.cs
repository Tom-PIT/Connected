using System;
using TomPIT.ComponentModel;

namespace TomPIT.Sys.SourceFiles;

public class ComponentIndexEntry : IComponent
{
	public string Name { get; set; }
	public Guid MicroService { get; set; }
	public Guid Token { get; set; }
	public string Type { get; set; }
	public string Category { get; set; }
	public DateTime Modified { get; set; }
	public Guid Folder { get; set; }
	public string NameSpace { get; set; }

	public static ComponentIndexEntry From(IComponent component)
	{
		return new ComponentIndexEntry
		{
			Category = component.Category,
			Folder = component.Folder,
			MicroService = component.MicroService,
			Modified = component.Modified,
			Name = component.Name,
			NameSpace = component.NameSpace,
			Token = component.Token,
			Type = component.Type
		};
	}
}
