using System;
using TomPIT.Cdn;

namespace TomPIT.Sys.Model.Printing
{
	internal class PrintJob : IPrintJob
	{
		public PrintJob()
		{

		}

		public PrintJob(IPrintJob item)
		{
			Token = item.Token;
			Status = item.Status;
			Error = item.Error;
			Created = item.Created;
			Provider = item.Provider;
			Arguments = item.Arguments;
			Component = item.Component;
			User = item.User;
			SerialNumber = item.SerialNumber;
			Category = item.Category;
			CopyCount = item.CopyCount;
		}

		public Guid Token { get; set; }

		public PrintJobStatus Status { get; set; }

		public string Error { get; set; }

		public DateTime Created { get; set; } = DateTime.UtcNow;

		public string Provider { get; set; }
		public string Arguments { get; set; }
		public Guid Component { get; set; }

		public string User { get; set; }

		public long SerialNumber { get; set; }

		public string Category { get; set; }

		public int CopyCount { get; set; }
	}
}
