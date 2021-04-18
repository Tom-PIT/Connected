namespace TomPIT.Cdn.Mail
{
	internal class ActionType
	{
		private sbyte _action;

		private const sbyte ConnectByte = 1;
		private const sbyte EhloByte = 2;
		private const sbyte MailByte = 3;
		private const sbyte RcptByte = 4;
		private const sbyte DataByte = 5;
		private const sbyte DataEndByte = 6;
		private const sbyte QuitByte = 7;
		private const sbyte UnrecByte = 8;
		private const sbyte BlankLineByte = 9;
		private const sbyte RsetByte = -1;
		private const sbyte VrfyByte = -2;
		private const sbyte ExpnByte = -3;
		private const sbyte HelpByte = -4;
		private const sbyte NoopByte = -5;

		public static readonly ActionType Connect = new ActionType(ConnectByte);
		public static readonly ActionType Ehlo = new ActionType(EhloByte);
		public static readonly ActionType Mail = new ActionType(MailByte);
		public static readonly ActionType Rcpt = new ActionType(RcptByte);
		public static readonly ActionType Data = new ActionType(DataByte);
		public static readonly ActionType DataEnd = new ActionType(DataEndByte);
		public static readonly ActionType Unrecog = new ActionType(UnrecByte);
		public static readonly ActionType Quit = new ActionType(QuitByte);
		public static readonly ActionType BlankLine = new ActionType(BlankLineByte);

		public static readonly ActionType Rset = new ActionType(RsetByte);
		public static readonly ActionType Vrfy = new ActionType(VrfyByte);
		public static readonly ActionType Expn = new ActionType(ExpnByte);
		public static readonly ActionType Help = new ActionType(HelpByte);
		public static readonly ActionType Noop = new ActionType(NoopByte);

		private ActionType(sbyte action)
		{
			_action = action;
		}

		public bool Stateless { get { return _action < 0; } }

		public override string ToString()
		{
			switch (_action)
			{
				case ConnectByte:
					return "Connect";
				case EhloByte:
					return "EHLO";
				case MailByte:
					return "MAIL";
				case RcptByte:
					return "RCPT";
				case DataByte:
					return "DATA";
				case DataEndByte:
					return ".";
				case QuitByte:
					return "QUIT";
				case RsetByte:
					return "RSET";
				case VrfyByte:
					return "VRFY";
				case ExpnByte:
					return "EXPN";
				case HelpByte:
					return "HELP";
				case NoopByte:
					return "NOOP";
				case UnrecByte:
					return "Unrecognized command / data";
				case BlankLineByte:
					return "Blank line";
				default:
					return "Unknown";
			}
		}
	}
}