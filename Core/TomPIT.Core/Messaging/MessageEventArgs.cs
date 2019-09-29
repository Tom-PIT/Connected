using System;

namespace TomPIT.Messaging
{
	public class MessageEventArgs<T> : EventArgs
	{
		public MessageEventArgs()
		{
		}

		public MessageEventArgs(Guid message, T args)
		{
			Message = message;
			Args = args;
		}

		public Guid Message { get; set; }
		public T Args { get; set; }
	}
}
