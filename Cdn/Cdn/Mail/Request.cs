namespace TomPIT.Cdn.Mail
{
	// -----------+-------------------------------------------------------------------------------------------------
	//                                  State
	// Action     +-------------+-----------+-----------+--------------+---------------+---------------+------------
	//			     | CONNECT     | GREET     | MAIL      | RCPT         | DATA_HDR      | DATA_BODY     | QUIT
	// -----------+-------------+-----------+-----------+--------------+---------------+---------------+------------
	// connect    | 220/GREET   | 503/GREET | 503/MAIL  | 503/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 503/QUIT
	// ehlo       | 503/CONNECT | 250/MAIL  | 503/MAIL  | 503/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 503/QUIT
	// mail       | 503/CONNECT | 503/GREET | 250/RCPT  | 503/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 250/RCPT
	// rcpt       | 503/CONNECT | 503/GREET | 503/MAIL  | 250/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 503/QUIT
	// data       | 503/CONNECT | 503/GREET | 503/MAIL  | 354/DATA_HDR | 503/DATA_HDR  | 503/DATA_BODY | 503/QUIT
	// data_end   | 503/CONNECT | 503/GREET | 503/MAIL  | 503/RCPT     | 250/QUIT      | 250/QUIT      | 503/QUIT
	// unrecog    | 500/CONNECT | 500/GREET | 500/MAIL  | 500/RCPT     | ---/DATA_HDR  | ---/DATA_BODY | 500/QUIT
	// quit       | 503/CONNECT | 503/GREET | 503/MAIL  | 503/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 250/CONNECT
	// blank_line | 503/CONNECT | 503/GREET | 503/MAIL  | 503/RCPT     | ---/DATA_BODY | ---/DATA_BODY | 503/QUIT
	// rset       | 250/GREET   | 250/GREET | 250/GREET | 250/GREET    | 250/GREET     | 250/GREET     | 250/GREET
	// vrfy       | 252/CONNECT | 252/GREET | 252/MAIL  | 252/RCPT     | 252/DATA_HDR  | 252/DATA_BODY | 252/QUIT
	// expn       | 252/CONNECT | 252/GREET | 252/MAIL  | 252/RCPT     | 252/DATA_HDR  | 252/DATA_BODY | 252/QUIT
	// help       | 211/CONNECT | 211/GREET | 211/MAIL  | 211/RCPT     | 211/DATA_HDR  | 211/DATA_BODY | 211/QUIT
	// noop       | 250/CONNECT | 250/GREET | 250/MAIL  | 250/RCPT     | 250|DATA_HDR  | 250/DATA_BODY | 250/QUIT
	internal class Request
	{

		public Request(ActionType actionType, string requestParams, State state)
		{
			Action = actionType;
			State = state;
			Params = requestParams;
		}

		public string Params { get; }
		private State State { get; }
		private ActionType Action { get; }
		public virtual Response Execute()
		{
			Response response = null;

			if (Action.Stateless)
			{
				if (ActionType.Expn == Action || ActionType.Vrfy == Action)
					response = new Response(252, "Not supported", State);
				else if (ActionType.Help == Action)
					response = new Response(211, "No help available", State);
				else if (ActionType.Noop == Action)
					response = new Response(250, "OK", State);
				else if (ActionType.Vrfy == Action)
					response = new Response(252, "Not supported", State);
				else if (ActionType.Rset == Action)
					response = new Response(250, "OK", State.Greet);
				else
					response = new Response(500, "Command not recognized", State);
			}
			else
			{
				if (ActionType.Connect == Action)
				{
					if (State.Connect == State)
						response = new Response(220, SmtpService.HostName, State.Greet);
					else
						response = new Response(503, "Bad sequence of commands: " + Action, State);
				}
				else if (ActionType.Ehlo == Action)
				{
					if (State.Greet == State)
						response = new Response(250, SmtpService.Greeting, State.Mail);
					else
						response = new Response(503, "Bad sequence of commands: " + Action, State);
				}
				else if (ActionType.Mail == Action)
				{
					if (State.Mail == State || State.Quit == State)
						response = new Response(250, "OK", State.Rcpt);
					else
						response = new Response(503, "Bad sequence of commands: " + Action, State);
				}
				else if (ActionType.Rcpt == Action)
				{
					if (State.Rcpt == State)
						response = new Response(250, "OK", State);
					else
						response = new Response(503, "Bad sequence of commands: " + Action, State);
				}
				else if (ActionType.Data == Action)
				{
					if (State.Rcpt == State)
						response = new Response(354, "Start mail input; end with <CRLF>.<CRLF>", State.DataHdr);
					else
						response = new Response(503, "Bad sequence of commands: " + Action, State);
				}
				else if (ActionType.Unrecog == Action)
				{
					if (State.DataHdr == State || State.DataBody == State)
						response = new Response(-1, "", State);
					else
						response = new Response(500, "Command not recognized", State);
				}
				else if (ActionType.DataEnd == Action)
				{
					if (State.DataHdr == State || State.DataBody == State)
						response = new Response(250, "OK", State.Quit);
					else
						response = new Response(503, "Bad sequence of commands: " + Action, State);
				}
				else if (ActionType.BlankLine == Action)
				{
					if (State.DataHdr == State)
						response = new Response(-1, "", State.DataBody);
					else if (State.DataBody == State)
						response = new Response(-1, "", State);
					else
						response = new Response(503, "Bad sequence of commands: " + Action, State);
				}
				else if (ActionType.Quit == Action)
				{
					if (State.Quit == State)
						response = new Response(221, "TomPIT service closing transmission channel", State.Connect);
					else
						response = new Response(503, "Bad sequence of commands: " + Action, State);
				}
				else
					response = new Response(500, "Command not recognized", State);
			}

			return response;
		}

		public static Request CreateRequest(string s, State state)
		{
			var parameters = string.Empty;
			ActionType action;

			if (state == State.DataHdr)
			{
				if (s.Equals("."))
					action = ActionType.DataEnd;
				else if (s.Length < 1)
					action = ActionType.BlankLine;
				else
				{
					action = ActionType.Unrecog;
					parameters = s;
				}
			}
			else if (state == State.DataBody)
			{
				if (s.Equals("."))
					action = ActionType.DataEnd;
				else
				{
					action = ActionType.Unrecog;
					parameters = s;
				}
			}
			else
			{
				var su = s.ToUpper();

				if (su.StartsWith("EHLO ") || su.StartsWith("HELO"))
				{
					action = ActionType.Ehlo;
					parameters = s.Substring(5);
				}
				else if (su.StartsWith("MAIL FROM:"))
				{
					action = ActionType.Mail;
					parameters = s.Substring(10);
				}
				else if (su.StartsWith("RCPT TO:"))
				{
					action = ActionType.Rcpt;
					parameters = s.Substring(8);
				}
				else if (su.StartsWith("DATA"))
					action = ActionType.Data;
				else if (su.StartsWith("QUIT"))
					action = ActionType.Quit;
				else if (su.StartsWith("RSET"))
					action = ActionType.Rset;
				else if (su.StartsWith("NOOP"))
					action = ActionType.Noop;
				else if (su.StartsWith("EXPN"))
					action = ActionType.Expn;
				else if (su.StartsWith("VRFY"))
					action = ActionType.Vrfy;
				else if (su.StartsWith("HELP"))
					action = ActionType.Help;
				else
					action = ActionType.Unrecog;
			}

			return new Request(action, parameters, state);
		}
	}
}