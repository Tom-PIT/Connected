using System;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Ide
{
	internal class Event : EnvironmentClient, IEvent
	{
		public Event(IEnvironment environment, IDomElement element) : base(environment)
		{
			Element = element;
		}

		public string Name { get; set; }
		public Guid Id { get; set; }
		public IDomElement Element { get; }
		public string Glyph { get; set; }
		public Guid Blob { get; set; }
	}
}
