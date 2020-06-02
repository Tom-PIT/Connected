namespace TomPIT.Cdn.Mail
{
	internal class Response
	{
		public Response(int code, string message, State next)
		{
			Code = code;
			Message = message;
			NextState = next;
		}
		public int Code { get; }
		public string Message { get; }
		public State NextState { get; }

		public static Response RcptMailboxUnavailable { get { return new Response(450, "Requested mail action not taken: mailbox unavailable", State.Quit); } }
		public static Response RcptMailboxNameNotAllowed { get { return new Response(553, "Requested action not taken: mailbox name not allowed", State.Quit); } }
	}
}
