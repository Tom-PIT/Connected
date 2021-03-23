using System;
using TomPIT.Design.Ide.Dom;

namespace TomPIT.Design.Ide.Events
{
	public interface IEvent : IEnvironmentClient, IDomObject
	{
		string Name { get; }
		Guid Id { get; }
		string Glyph { get; }
		Guid Blob { get; }
	}
}
