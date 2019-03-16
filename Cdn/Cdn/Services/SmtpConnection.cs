using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Net.Sockets;
using System.Threading;
using TomPIT.Cdn.Dns;
using TomPIT.Diagnostics;
using TomPIT.Services;

namespace TomPIT.Cdn.Services
{
	internal enum ConnectionState
	{
		Idle = 1,
		Active = 2
	}

	internal class SmtpConnection : IDisposable
	{
		private SmtpClient _client = null;
		private ConnectionState _state = ConnectionState.Idle;

		public SmtpConnection(string domainName)
		{
			Domain = domainName;
		}

		private string Domain { get; }
		public ConnectionState State
		{
			get { return _state; }
			set
			{
				if (value == ConnectionState.Active)
					TimeStamp = DateTime.UtcNow;

				_state = value;
			}
		}

		public DateTime TimeStamp { get; private set; } = DateTime.UtcNow;

		public void Disconnect()
		{
			try
			{
				if (_client == null)
					return;

				if (_client.IsConnected)
					_client.Disconnect(true);
			}
			catch (Exception ex)
			{
				State = ConnectionState.Idle;
				Instance.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "SmtpConnection Disconnect",
					EventId = ExecutionEvents.SendMail
				});
			}
		}

		public void Connect(CancellationToken token)
		{
			try
			{
				if (_client == null)
					_client = new SmtpClient();

				if (_client.IsConnected)
					return;

				string server = DnsResolve.Resolve(Domain);

				if (string.IsNullOrWhiteSpace(server))
					throw new SmtpException(SmtpExceptionType.CannotResolveDomain, Domain);

				//_client.LocalDomain = "tompit.net";
				_client.Connect(new Uri(string.Format("smtp://{0}", server)), token);
				//_client.Connect(server, 587, MailKit.Security.SecureSocketOptions.StartTls, token);
			}
			catch (SocketException ex)
			{
				State = ConnectionState.Idle;
				DnsResolve.Reset(Domain);

				Instance.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "SmtpConnection Connect",
					EventId = ExecutionEvents.SendMail
				});

				throw new SmtpException(SmtpExceptionType.ConnectionFailure, ex);
			}
			catch (SmtpException)
			{
				State = ConnectionState.Idle;
				throw;
			}
			catch (OperationCanceledException)
			{
				State = ConnectionState.Idle;
				throw;
			}
			catch (Exception ex)
			{
				State = ConnectionState.Idle;
				Instance.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "SmtpConnection Connect",
					EventId = ExecutionEvents.SendMail
				});


				throw new SmtpException(SmtpExceptionType.ConnectionFailure, ex);
			}
		}

		public void Send(CancellationToken token, MimeMessage message, string email)
		{
			Connect(token);

			try
			{
				if (_client.IsConnected)
				{
					var recipient = new MailboxAddress(string.Empty, email);

					_client.Send(FormatOptions.Default, message, message.Sender, new MailboxAddress[] { recipient }, token);
				}
				else
					throw new SmtpException(SmtpExceptionType.NotConnected);
			}
			catch (SmtpException)
			{
				throw;
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (SmtpCommandException ex)
			{
				switch (ex.ErrorCode)
				{
					case SmtpErrorCode.MessageNotAccepted:
						throw new SmtpException(SmtpExceptionType.MessageNotAccepted, ex);
					case SmtpErrorCode.RecipientNotAccepted:
						throw new SmtpException(SmtpExceptionType.RecipientNotAccepted, ex);
					case SmtpErrorCode.SenderNotAccepted:
						throw new SmtpException(SmtpExceptionType.SenderNotAccepted, ex);
					case SmtpErrorCode.UnexpectedStatusCode:
						throw new SmtpException(SmtpExceptionType.UnexpectedStatusCode, ex);
					default:
						throw new SmtpException(SmtpExceptionType.SendFailure, ex);
				}
			}
			catch (UnauthorizedAccessException ex1)
			{
				throw new SmtpException(SmtpExceptionType.UnauthorizedAccess, ex1);
			}
			catch (Exception ex2)
			{
				throw new SmtpException(SmtpExceptionType.SendFailure, ex2);
			}
			finally
			{
				State = ConnectionState.Idle;
			}
		}

		public void Dispose()
		{
			if (_client == null)
				return;

			if (_client.IsConnected)
				_client.Disconnect(true);

			_client.Dispose();
			_client = null;
		}

		public bool IsConnected(CancellationToken token)
		{
			try
			{
				Connect(token);

				return _client.IsConnected;
			}
			catch
			{
				return false;
			}
		}
	}
}