using System;
using TomPIT.Ide.Dom;

namespace TomPIT.Ide.Messaging
{
	public interface IEvent : IEnvironmentClient, IDomObject
	{
		string Name { get; }
		Guid Id { get; }
		string Glyph { get; }
		Guid Blob { get; }
	}
}
