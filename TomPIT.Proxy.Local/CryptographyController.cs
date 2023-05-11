using TomPIT.Security;

namespace TomPIT.Proxy.Local
{
	internal class CryptographyController : ICryptographyController
	{
		public string Decrypt(string cipherText)
		{
			return Shell.GetService<ISysCryptographyService>().Decrypt(this, cipherText);
		}

		public string Encrypt(string plainText)
		{
			return Shell.GetService<ISysCryptographyService>().Encrypt(this, plainText);
		}

		public string Hash(string value)
		{
			return Shell.GetService<ISysCryptographyService>().Hash(value);
		}

		public bool VerifyHash(string value, string existing)
		{
			return Shell.GetService<ISysCryptographyService>().VerifyHash(value, existing);
		}
	}
}
