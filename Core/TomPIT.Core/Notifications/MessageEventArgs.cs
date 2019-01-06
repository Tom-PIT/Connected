using System;

namespace TomPIT.Notifications
{
	public class MessageEventArgs<T> : EventArgs
	{
		public MessageEventArgs(Guid message, T args)
		{
			Message = message;
			Args = args;
		}

		public Guid Message { get; }
		public T Args { get; }
	}
}
