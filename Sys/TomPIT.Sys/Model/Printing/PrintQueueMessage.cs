using System;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.Sys.Model.Cdn;

namespace TomPIT.Sys.Model.Printing
{
	internal class PrintQueueMessage : QueueMessage, IPrintQueueMessage
	{
		public PrintQueueMessage()
		{

		}
		public PrintQueueMessage(IQueueMessage message, IPrintJob job) : base(message)
		{
			Token = job.Token;
			Status = job.Status;
			Error = job.Error;
			Provider = job.Provider;
			Arguments = job.Arguments;
			Component = job.Component;
			User = job.User;
			SerialNumber = job.SerialNumber;
			Category = job.Category;
			CopyCount = job.CopyCount;
		}

		public Guid Token { get; set; }
		public PrintJobStatus Status { get; set; }
		public string Error { get; set; }
		public string Provider { get; set; }
		public string Arguments { get; set; }
		public Guid Component { get; set; }
		public string User { get; set; }
		public long SerialNumber { get; set; }
		public string Category { get; set; }
		public int CopyCount { get; set; }
	}
}
