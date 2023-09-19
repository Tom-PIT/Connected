using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public class XmlKeyEncryptor : IXmlEncryptor, IXmlDecryptor
	{
		public XElement Decrypt(XElement encryptedElement)
		{
			var encryptedContent = encryptedElement.Value;

			var decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedContent));

			var decryptedContent = MiddlewareDescriptor.Current.Tenant.GetService<ICryptographyService>().Decrypt(decodedContent);

			return XElement.Parse(decryptedContent);
		}

		public EncryptedXmlInfo Encrypt(XElement plaintextElement)
		{
			using var ms = new MemoryStream();

			plaintextElement.Save(ms, SaveOptions.DisableFormatting);
			ms.Position = 0;

			using var sr = new StreamReader(ms);

			var encrypted = MiddlewareDescriptor.Current.Tenant.GetService<ICryptographyService>().Encrypt(sr.ReadToEnd());

			var element = new XElement("encryptedKey",
				 new XComment(" This key is encrypted with Tom PIT Cryptography service. "),
				 new XElement("value", Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted)))
			);

			return new EncryptedXmlInfo(element, typeof(XmlKeyEncryptor));
		}
	}
}
