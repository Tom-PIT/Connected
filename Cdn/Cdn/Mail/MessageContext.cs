using System.IO;
using MimeKit;

namespace TomPIT.Cdn.Mail
{
	internal class MessageContext
	{
		public MessageContext(MemoryStream content)
		{
			try
			{
				Message = MimeMessage.Load(content);
				Length = content.Length;
			}
			catch { }
		}

		public long Length { get; }

		public MimeMessage Message { get; }
	}
}