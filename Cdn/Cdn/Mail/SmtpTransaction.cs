using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MimeKit;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Mail
{
	internal class SmtpTransaction : IDisposable
	{
		private Socket _socket = null;
		private List<AuthorizedRecipient> _recipients = null;
		public SmtpTransaction(Socket socket)
		{
			State = State.Connect;
			_socket = socket;
		}

		public void ProcessRequest()
		{
			try
			{
				Initialize();

				using var ms = new MemoryStream();
				using var writer = new StreamWriter(ms);

				var completed = false;

				while (State != State.Connect)
				{
					var line = Input.ReadLine();

					if (line == null)
						break;

					var respond = true;

					if (State == State.Rcpt)
						respond = ParseRecipient(line);

					var request = Request.CreateRequest(line, State);
					var response = request.Execute();

					State = response.NextState;

					if (State.IsWritable)
					{
						if (!State.IsDataHeader || (State.IsDataHeader && !string.IsNullOrWhiteSpace(request.Params)))
							writer.WriteLine(line);
					}

					var processorResult = InboxMessageResult.OK;

					if (State == State.Quit && !completed)
					{
						writer.Flush();

						ms.Seek(0, SeekOrigin.Begin);

						var ip = _socket.RemoteEndPoint as IPEndPoint;

						processorResult = MailProcessor.ProcessMail(ms, Recipients);

						completed = true;
					}

					if (respond)
					{
						if (processorResult != InboxMessageResult.OK)
							SendResponse(processorResult);
						else
							SendResponse(response);
					}
				}
			}
			catch (SocketException ex)
			{
				//10054 = Connection reset by peer. An existing connection was forcibly closed by the remote host. 
				//10060 = Connection timed out. A connection attempt failed because the connected party did not properly respond after a period of time, or the established connection failed because the connected host has failed to respond
				if (ex.ErrorCode != 10054 && ex.ErrorCode != 10060)
					MiddlewareDescriptor.Current.Tenant.LogError(nameof(SmtpTransaction), ex.Message, LogCategories.Cdn);

				if (Output != null)
				{
					try
					{
						SendResponse(InboxMessageResult.Error);
					}
					catch { }
				}
			}
			catch (IOException)
			{
				//eat probably closed by remote host.
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError(nameof(SmtpTransaction), ex.Message, LogCategories.Cdn);

				if (Output != null)
				{
					try { SendResponse(InboxMessageResult.Error); }
					catch { }
				}
			}
		}

		private bool ParseRecipient(string headerLine)
		{
			var tokens = headerLine.Split(':');

			if (tokens != null && tokens.Length > 1)
			{
				var hdr = string.Format("{0} TO", State.Rcpt);

				if (string.Compare(tokens[0].Trim(), hdr, true) == 0)
				{
					var recipient = AuthorizeRecipient(tokens[1]);

					if (recipient != null)
					{
						Recipients.Add(recipient);

						return true;
					}
					else
						return false;
				}
			}

			return true;
		}
		private AuthorizedRecipient AuthorizeRecipient(string email)
		{
			Response response = null;
			AuthorizedRecipient result = null;
			InternetAddress address = null;

			if (!InternetAddress.TryParse(email, out address))
				response = Response.RcptMailboxUnavailable;
			else
			{
				if (!(address is MailboxAddress ma))
					response = Response.RcptMailboxUnavailable;
				else
				{
					var ctx = email.Split('@')[0];

					result = new AuthorizedRecipient
					{
						Email = ma
					};

					if (string.Compare(ctx, "bounce", true) == 0)
						result.Kind = RecipientKind.Bounce;
					else if (string.Compare(ctx, "abuse", true) == 0)
						result.Kind = RecipientKind.Abuse;
					else
						result.Kind = RecipientKind.Content;
				}
			}

			if (response != null)
			{
				SendResponse(Response.RcptMailboxUnavailable);

				return null;
			}
			else
				return result;
		}

		private void Initialize()
		{
			Request = new Request(ActionType.Connect, string.Empty, State);
			Response = Request.Execute();
			Stream = new NetworkStream(_socket);
			Input = new StreamReader(Stream, true);
			Output = new StreamWriter(Stream);

			SendResponse(Response);

			State = Response.NextState;
		}
		public void Dispose()
		{
			if (Output != null)
			{
				Output.Dispose();
				Output = null;
			}

			Stream = null;
			Input = null;

			_socket = null;
		}

		private void SendResponse(InboxMessageResult result)
		{
			if (Output == null)
				return;

			string line = string.Empty;

			switch (result)
			{
				case InboxMessageResult.OK:
					line = string.Format("{0} {1}", (int)result, "OK");
					break;
				case InboxMessageResult.MailboxNotFound:
					line = string.Format("{0} {1}", (int)result, "Requested action not taken: mailbox unavailable");
					break;
				case InboxMessageResult.AccessDenied:
					line = string.Format("{0} {1}", (int)result, "Access denied");
					break;
				case InboxMessageResult.MessageTooLarge:
					line = string.Format("{0} {1}", (int)result, "Requested action aborted: exceeded storage allocation");
					break;
				case InboxMessageResult.Error:
					line = string.Format("{0} {1}", (int)result, "Requested action not taken: local error in processing");
					break;
				case InboxMessageResult.NotImplemented:
					line = string.Format("{0} {1}", (int)result, "Command not implemented");
					break;
				case InboxMessageResult.SyntaxErrorInParameters:
					line = string.Format("{0} {1}", (int)result, "Syntax error in parameters or arguments");
					break;
			}

			if (!string.IsNullOrWhiteSpace(line))
			{
				Output.WriteLine(line);
				Output.Flush();
			}
		}

		private void SendResponse(Response smtpResponse)
		{
			if (Output == null)
				return;

			if (smtpResponse.Code > 0)
			{
				string line = string.Format("{0} {1}", smtpResponse.Code, smtpResponse.Message);

				Output.WriteLine(line);
				Output.Flush();
			}
		}

		private Request Request { get; set; }
		private Response Response { get; set; }
		private NetworkStream Stream { get; set; }
		private StreamWriter Output { get; set; }
		private State State { get; set; }
		private StreamReader Input { get; set; }

		private List<AuthorizedRecipient> Recipients
		{
			get
			{
				if (_recipients == null)
					_recipients = new List<AuthorizedRecipient>();

				return _recipients;
			}
		}
	}
}