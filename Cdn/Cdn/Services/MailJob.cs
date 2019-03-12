using MimeKit;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using TomPIT.Diagnostics;
using TomPIT.Services;

namespace TomPIT.Cdn.Services
{
	internal class MailJob : DispatcherJob<IMailMessage>
	{
		public MailJob(Dispatcher<IMailMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IMailMessage item)
		{
			var domain = item.ReceiverDomain();

			if (string.IsNullOrWhiteSpace(domain))
			{
				Instance.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = string.Format("{0} ({1})", SR.ErrInvalidReceiverAddress, item.To),
					Source = "SmtpConnection Connect",
					EventId = ExecutionEvents.SendMail
				});
			}

			var connection = ConnectionPool.Request(domain);

			if (connection == null)
			{
				Owner.Enqueue(item);
				return;
			}

			SendMail(connection, item);
		}

		private void SendMail(SmtpConnection connection, IMailMessage item)
		{
			try
			{
				var address = MailboxAddress.Parse(item.To);
				var message = new MailProcessor(item);

				message.Create();

				connection.Connect(Cancel.Token);
				connection.Send(Cancel.Token, message.Message, address.Address);

				Success(item);
			}
			catch (SmtpException ex)
			{
				var policy = new ResendPolicy(ex);

				Fail(item, ex.Message, policy.Delay);
			}
			catch (OperationCanceledException)
			{
				Fail(item, SR.ErrSendMailCancelled, 60);
			}
			catch (Exception ex)
			{
				Fail(item, SR.ErrSendMailCancelled, 60);

				Instance.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "SmtpConnection Connect",
					EventId = ExecutionEvents.SendMail
				});
			}
		}

		private void Fail(IMailMessage message, string error, int delay)
		{
			var u = Instance.Connection.CreateUrl("MailManagement", "Update");
			var e = new JObject
			{
				{"popReceipt", message.PopReceipt },
				{"error", error },
				{"delay", delay }
			};

			Instance.Connection.Post(u, e);
		}

		private void Success(IMailMessage message)
		{
			var u = Instance.Connection.CreateUrl("MailManagement", "DeleteByPopReceipt");
			var e = new JObject
			{
				{"popReceipt", message.PopReceipt }
			};

			Instance.Connection.Post(u, e);
		}
	}
}
