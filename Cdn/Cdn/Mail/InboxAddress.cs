namespace TomPIT.Cdn.Mail
{
	internal class InboxAddress : IInboxAddress
	{
		public InboxAddress()
		{

		}

		public InboxAddress(string name, string address) : this(name, address, false)
		{
		}

		public InboxAddress(string name, string address, bool isInternational)
		{
			Name = name;
			Address = address;
			IsInternational = isInternational;
		}
		public string Name { get; set; }

		public string Address { get; set; }

		public bool IsInternational { get; set; }
	}
}
