namespace TomPIT.Cdn.Mail
{
	internal class InboxAttachment : IInboxAttachment
	{
		public string MediaType { get; set; }

		public string ContentType { get; set; }

		public string MediaSubtype { get; set; }

		public string Charset { get; set; }

		public string Name { get; set; }

		public byte[] Content { get; set; }
	}
}
