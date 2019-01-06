using System;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IEvent : IEnvironmentClient, IDomClient
	{
		string Name { get; }
		Guid Id { get; }
		string Glyph { get; }
		Guid Blob { get; }
	}
}
