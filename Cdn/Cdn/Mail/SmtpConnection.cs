using System;
using System.Net.Sockets;
using System.Threading;
using MailKit.Net.Smtp;
using MimeKit;
using TomPIT.Cdn.Dns;
using TomPIT.Configuration;
using TomPIT.Diagnostics;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Mail
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
		private static SmtpConnectionConfigurationCache _configCache = null;

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
				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "SmtpConnection Disconnect",
					EventId = MiddlewareEvents.SendMail
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

				var server = string.Empty;
				var localDomain = string.Empty;

				foreach (var middleware in ConfigurationCache.Handlers)
				{
					var descriptor = middleware.ResolveSmtpServer(new DomainEventArgs(Domain));

					if (descriptor != null)
					{
						server = descriptor.Server;
						localDomain = descriptor.LocalDomain;

						break;
					}
				}

				if (string.IsNullOrWhiteSpace(server))
					server = MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().GetValue<string>(Guid.Empty, "SmtpServer");

				if (string.IsNullOrWhiteSpace(server))
					server = DnsResolve.Resolve(Domain);

				if (string.IsNullOrWhiteSpace(server))
					throw new SmtpException(SmtpExceptionType.CannotResolveDomain, Domain);

				_client.LocalDomain = SmtpService.HostName;
				_client.Connect(new Uri(string.Format("smtp://{0}", server)), token);

				foreach (var middleware in ConfigurationCache.Handlers)
				{
					var credentials = middleware.GetCredentials(new SmtpCredentialsEventArgs(Domain));

					if (credentials != null)
					{
						if (credentials is ISmtpBasicCredentials basic)
						{
							if (credentials.Encoding != null)
								_client.Authenticate(credentials.Encoding, basic.UserName, basic.Password);
							else
								_client.Authenticate(basic.UserName, basic.Password);
						}
						else if (credentials is ISmtpNetworkCredentials network)
						{
							if (credentials.Encoding != null)
								_client.Authenticate(credentials.Encoding, network.Credentials);
							else
								_client.Authenticate(network.Credentials);
						}
						else
							throw new NotSupportedException();

						break;
					}
				}
			}
			catch (SocketException ex)
			{
				State = ConnectionState.Idle;
				DnsResolve.Reset(Domain);

				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "SmtpConnection Connect",
					EventId = MiddlewareEvents.SendMail
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
				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "SmtpConnection Connect",
					EventId = MiddlewareEvents.SendMail
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

		private static SmtpConnectionConfigurationCache ConfigurationCache
		{
			get
			{
				if (_configCache == null)
					_configCache = new SmtpConnectionConfigurationCache(MiddlewareDescriptor.Current.Tenant);

				return _configCache;
			}
		}
	}
}