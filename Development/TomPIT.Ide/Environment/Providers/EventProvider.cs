using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Events;
using TomPIT.Design.Ide.Properties;
using TomPIT.Ide.Messaging;
using TomPIT.Reflection;

namespace TomPIT.Ide.Environment.Providers
{
	internal class EventProvider : EnvironmentObject, IEventProvider
	{
		private List<IEvent> _events = null;

		public EventProvider(IEnvironment environment) : base(environment)
		{
		}

		public List<IEvent> Events
		{
			get
			{
				if (_events == null)
				{
					if (Environment.Selection.Element == null)
						return null;

					_events = new List<IEvent>();

					IPropertySource source = null;

					if (Environment.Selection.Element != null)
						source = Environment.Selection.Element as IPropertySource;

					if (source != null)
					{
						var instances = source.PropertySources;

						if (instances != null)
						{
							foreach (var i in instances)
								QueryEvents(Environment.Selection.Element, i);
						}
					}
					else
						QueryEvents(Environment.Selection.Element, Environment.Selection.Element.Value);

					_events = _events.OrderBy(f => f.Name).ToList();
				}

				return _events;
			}
		}

		private void QueryEvents(IDomElement element, object instance)
		{
			if (instance == null)
				return;

			var props = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);

			foreach (var i in props)
			{
				if (DomQuery.ImplementsInterface<TomPIT.ComponentModel.IEvent>(i.PropertyType))
				{
					if (!(i.GetValue(instance) is TomPIT.ComponentModel.IEvent d))
						continue;

					var args = i.FindAttribute<EventArgumentsAttribute>();

					if (args == null)
						continue;

					var glyph = d.GetType().FindAttribute<GlyphAttribute>();

					var e = new EventObject(element)
					{
						Id = d.Id,
						Name = i.Name,
						Glyph = glyph == null ? string.Empty : glyph.Glyph,
						Blob = d.TextBlob
					};

					_events.Add(e);
				}
			}
		}
	}
}
