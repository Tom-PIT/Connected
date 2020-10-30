using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public enum InboxMessageResult
	{
		Cancelled = 0,
		OK = 250,
		SyntaxErrorInParameters = 501, //Syntax error in parameters or arguments
		MailboxNotFound = 550,//Requested action not taken: mailbox unavailable
		AccessDenied = 530,//Access denied
		MessageTooLarge = 552,//Requested action aborted: exceeded storage allocation
		Error = 451,//Requested action not taken: local error in processing
		NotImplemented = 502//Command not implemented
	}

	public interface IInboxMiddleware : IMiddlewareComponent
	{
		List<IInboxAddress> Addresses { get; }

		InboxMessageResult Invoke(IInboxMessage message);
	}
}
