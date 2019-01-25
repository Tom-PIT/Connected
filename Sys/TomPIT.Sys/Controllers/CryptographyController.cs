using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;

namespace TomPIT.Sys.Controllers
{
	public class CryptographyController : SysController
	{
		[HttpPost]
		public string Encrypt()
		{
			var body = FromBody();

			var plainText = body.Required<string>("plainText");

			return Shell.GetService<ICryptographyService>().Encrypt(this, plainText);
		}

		[HttpPost]
		public string Decrypt()
		{
			var body = FromBody();

			var cipherText = body.Required<string>("cipherText");

			return Shell.GetService<ICryptographyService>().Decrypt(this, cipherText);
		}

		[HttpPost]
		public string Hash()
		{
			var b = FromBody();

			var value = b.Required<string>("value");

			return Shell.GetService<ICryptographyService>().Hash(value);
		}

		[HttpPost]
		public bool VerifyHash()
		{
			var b = FromBody();

			var value = b.Required<string>("value");
			var existing = b.Required<string>("existing");

			return Shell.GetService<ICryptographyService>().VerifyHash(value, existing);
		}
	}
}
