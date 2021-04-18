using System;

namespace TomPIT.Cdn.Printing
{
	internal class PrintNotificationDescriptor
	{
		public Guid Id { get; set; }
		public Guid PopReceipt { get; set; }
		public long SerialNumber { get; set; }
		public string Printer { get; set; }
	}
}
