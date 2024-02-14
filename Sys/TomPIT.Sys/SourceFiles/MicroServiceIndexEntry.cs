using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Sys.SourceFiles;

public class MicroServiceIndexEntry : IMicroService
{
	public string Name { get; set; }
	public string Url { get; set; }
	public Guid Token { get; set; }
	public MicroServiceStages SupportedStages { get; set; }
	public Guid ResourceGroup { get; set; }
	public Guid Template { get; set; }
	public string Version { get; set; }
	public string Commit { get; set; }

	public List<ComponentIndexEntry> Components { get; set; }

	public static MicroServiceIndexEntry From(IMicroService microService, List<ComponentIndexEntry> components)
	{
		return new MicroServiceIndexEntry
		{
			Commit = microService.Commit,
			Components = components,
			Name = microService.Name,
			ResourceGroup = microService.ResourceGroup,
			SupportedStages = microService.SupportedStages,
			Template = microService.Template,
			Token = microService.Token,
			Url = microService.Url,
			Version = microService.Version
		};
	}
}
