using System;

namespace TomPIT.Cdn.Services
{
	public enum SmtpExceptionType
	{
		Undefined = 0,
		MessageNull = 1,
		NoReceivers = 2,
		AttachmentsLoadFailure = 3,
		DkimSignFailed = 4,
		CannotResolveDomain = 5,
		ConnectionFailure = 6,
		SendFailure = 7,
		UnauthorizedAccess = 8,
		MessageNotAccepted = 9,
		RecipientNotAccepted = 10,
		SenderNotAccepted = 11,
		UnexpectedStatusCode = 12,
		MailboxNull = 13,
		NotConnected = 14,
		NotSent = 15,
		Cancelled = 16
	}

	internal class SmtpException : Exception
	{
		public SmtpException() { }
		public SmtpException(SmtpExceptionType type, string argument)
			: this(type)
		{
			Argument = argument;
		}

		public SmtpException(SmtpExceptionType type)
		{
			Type = type;
		}

		public SmtpException(SmtpExceptionType type, Exception inner)
			: base(inner?.Message, inner)
		{
			Type = type;
		}

		public SmtpExceptionType Type { get; } = SmtpExceptionType.Undefined;
		private string Argument { get; }

		public override string Message
		{
			get
			{
				string m = string.Empty;

				if (string.IsNullOrWhiteSpace(Argument))
					m = Type.ToString();
				else
					m = string.Format("{0} {1}", Type.ToString(), Argument);

				if (string.IsNullOrWhiteSpace(base.Message))
					return m;
				else
					return string.Format("{0}{1}{2}", m, System.Environment.NewLine, base.Message);
			}
		}
	}
}
