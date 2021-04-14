using System;
using System.Threading;
using MimeKit;
using Newtonsoft.Json.Linq;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Mail
{
	internal class MailJob : DispatcherJob<IMailMessage>
	{
		public MailJob(IDispatcher<IMailMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IMailMessage item)
		{
			if (item.Expire <= DateTime.UtcNow)
				return;

			var domain = item.ReceiverDomain();

			if (string.IsNullOrWhiteSpace(domain))
			{
				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = string.Format("{0} ({1})", SR.ErrInvalidReceiverAddress, item.To),
					Source = "SmtpConnection Connect",
					EventId = MiddlewareEvents.SendMail
				});
			}

			var connection = SmtpConnectionPool.Request(domain);

			if (connection == null)
			{
				Thread.Sleep(2500);
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
				var message = new MailProcessor(item, ((MailDispatcher)Owner).ResourceGroup);

				message.Create();

				connection.Connect(Cancel);
				connection.Send(Cancel, message.Message, address.Address);

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

				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "SmtpConnection Connect",
					EventId = MiddlewareEvents.SendMail
				});
			}
		}

		private void Fail(IMailMessage message, string error, int delay)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("MailManagement", "Update");
			var e = new JObject
			{
				{"popReceipt", message.PopReceipt },
				{"error", error },
				{"delay", delay }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		private void Success(IMailMessage message)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("MailManagement", "DeleteByPopReceipt");
			var e = new JObject
			{
				{"popReceipt", message.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		protected override void OnError(IMailMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(MailJob));
		}
	}
}
