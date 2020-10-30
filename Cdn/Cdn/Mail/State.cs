namespace TomPIT.Cdn.Mail
{
	internal class State
	{
		private sbyte _state;

		private const sbyte ConnectByte = 1;
		private const sbyte GreetByte = 2;
		private const sbyte MailByte = 3;
		private const sbyte RcptByte = 4;
		private const sbyte DataHeaderByte = 5;
		private const sbyte DataBodyByte = 6;
		private const sbyte QuitByte = 7;

		public static readonly State Connect = new State(ConnectByte);
		public static readonly State Greet = new State(GreetByte);
		public static readonly State Mail = new State(MailByte);
		public static readonly State Rcpt = new State(RcptByte);
		public static readonly State DataHdr = new State(DataHeaderByte);
		public static readonly State DataBody = new State(DataBodyByte);
		public static readonly State Quit = new State(QuitByte);

		private State(sbyte state)
		{
			_state = state;
		}

		public bool IsDataHeader { get { return _state == DataHeaderByte; } }
		public bool IsWritable { get { return _state == DataHeaderByte || _state == DataBodyByte; } }
		public override string ToString()
		{
			switch (_state)
			{
				case ConnectByte:
					return "CONNECT";
				case GreetByte:
					return "GREET";
				case MailByte:
					return "MAIL";
				case RcptByte:
					return "RCPT";
				case DataHeaderByte:
					return "DATA_HDR";
				case DataBodyByte:
					return "DATA_BODY";
				case QuitByte:
					return "QUIT";
				default:
					return "Unknown";
			}
		}
	}
}
