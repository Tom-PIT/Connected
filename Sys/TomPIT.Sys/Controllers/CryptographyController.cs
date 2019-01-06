using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;

namespace TomPIT.Sys.Controllers
{
	public class CryptographyController : SysController
	{
		[HttpPost]
		public string Encrypt(string plainText)
		{
			return Shell.GetService<ICryptographyService>().Encrypt(this, plainText);
		}

		[HttpPost]
		public string Decrypt(string cipherText)
		{
			return Shell.GetService<ICryptographyService>().Decrypt(this, cipherText);
		}

		[HttpPost]
		public string Encrypt()
		{
			var b = FromBody();

			var e = new EncryptionEventArgs
			{
				Value = b.Required<string>("value"),
				PassPhrase = b.Required<string>("passPhrase"),
				SaltValue = b.Required<string>("saltValue"),
				HashAlgorithm = b.Required<string>("hashAlgorithm"),
				PasswordIterations = b.Required<int>("passwordIterations"),
				InitVector = b.Required<string>("initVector"),
				KeySize = b.Required<int>("keySize")
			};

			return Shell.GetService<ICryptographyService>().Encrypt(this, e);
		}

		[HttpPost]
		public string Decrypt()
		{
			var b = FromBody();

			var e = new EncryptionEventArgs
			{
				Value = b.Required<string>("value"),
				PassPhrase = b.Required<string>("passPhrase"),
				SaltValue = b.Required<string>("saltValue"),
				HashAlgorithm = b.Required<string>("hashAlgorithm"),
				PasswordIterations = b.Required<int>("passwordIterations"),
				InitVector = b.Required<string>("initVector"),
				KeySize = b.Required<int>("keySize")
			};

			return Shell.GetService<ICryptographyService>().Decrypt(this, e);

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
