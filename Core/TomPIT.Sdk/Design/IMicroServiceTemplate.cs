using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;

namespace TomPIT.Design
{
	public enum TemplateKind
	{
		Standalone = 1,
		Plugin = 2
	}

	public interface IMicroServiceTemplate : IMicroServiceTemplateDescriptor
	{
		List<IItemDescriptor> ProvideAddItems(IDomElement parent);
		List<IItemDescriptor> ProvideGlobalAddItems(IDomElement parent);
		IComponent References(IEnvironment environment, Guid microService);
		List<string> GetApplicationParts();
		List<IIdeResource> ProvideIdeResources();
		void Initialize(IApplicationBuilder app, IWebHostEnvironment env);

		TemplateKind Kind { get; }

		void RegisterRoutes(IEndpointRouteBuilder builder);
	}
}
