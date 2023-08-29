using System;
using TomPIT.ComponentModel;
using TomPIT.Design;

namespace TomPIT.Compilation;
public class DocumentIdentity : IDocumentIdentity
{
	private DocumentIdentity()
	{

	}
	public Guid MicroService { get; init; }

	public Guid Component { get; init; }

	public Guid Element { get; init; }

	public static DocumentIdentity From(IText text)
	{
		var ms = text.Configuration().MicroService();
		var component = text.Configuration().Component;

		return From(ms, component, text.Id);
	}

	public static DocumentIdentity From(Guid microService, Guid component, Guid element)
	{
		return new DocumentIdentity
		{
			MicroService = microService,
			Component = component,
			Element = element
		};
	}
}
