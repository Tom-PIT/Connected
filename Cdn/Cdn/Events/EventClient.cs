using System;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Cdn.Events
{
	public class EventClient:IComparable<EventClient>
	{
		private string _serializedArguments = null;
		public string ConnectionId { get; set; }
		public Guid User { get; set; }

		public string EventName { get; set; }
		public JObject Arguments { get; set; }
		public string Client { get; set; }

		public DateTime RetentionDeadline { get; set; }

		private string SerializedArguments
		{
			get
			{
				if (Arguments == null)
					return null;

				if (_serializedArguments == null)
					_serializedArguments = Serializer.Serialize(Arguments);

				return _serializedArguments;
			}
		}
		public EventSubscriptionBehavior Behavior { get; set; } = EventSubscriptionBehavior.Reliable;

		public int CompareTo(EventClient other)
		{
			if (other == null)
				return 1;

			if (User != other.User)
				return 1;

			if (string.Compare(EventName, other.EventName, true) != 0)
				return 1;

			if (string.Compare(Client, other.Client, true) != 0)
				return 1;

			if (Behavior != other.Behavior)
				return 1;

			if (Arguments == null && other.Arguments != null)
				return 1;

			if (Arguments != null && other.Arguments == null)
				return 1;

			return string.Compare(SerializedArguments, other.SerializedArguments, false);
		}
	}
}
