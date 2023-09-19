using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class CryptographyService : TenantObject, ICryptographyService
	{
		public CryptographyService(ITenant tenant) : base(tenant)
		{

		}
		public string Decrypt(string cipherText)
		{
			return Instance.SysProxy.Cryptography.Decrypt(cipherText);
		}

		public string Encrypt(string plainText)
		{
			return Instance.SysProxy.Cryptography.Encrypt(plainText);
		}

		public string Hash(string value)
		{
			return Instance.SysProxy.Cryptography.Hash(value);
		}

		public bool VerifyHash(string value, string existing)
		{
			return Instance.SysProxy.Cryptography.VerifyHash(value, existing);
		}
	}
}
