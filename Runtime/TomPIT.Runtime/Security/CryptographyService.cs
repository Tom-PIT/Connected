using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	internal class CryptographyService : TenantObject, ICryptographyService
	{
		public CryptographyService(ITenant tenant) : base(tenant)
		{

		}
		public string Decrypt(string cipherText)
		{
			var u = Tenant.CreateUrl("Cryptography", "Decrypt");
			var e = new JObject
			{
				{"cipherText", cipherText}
			};

			return Tenant.Post<string>(u, e);
		}

		public string Encrypt(string plainText)
		{
			var u = Tenant.CreateUrl("Cryptography", "Encrypt");
			var e = new JObject
			{
				{"plainText", plainText}
			};

			return Tenant.Post<string>(u, e);
		}

		public string Hash(string value)
		{
			var u = Tenant.CreateUrl("Cryptography", "Hash");
			var e = new JObject
			{
				{"value", value}
			};

			return Tenant.Post<string>(u, e);
		}

		public bool VerifyHash(string value, string existing)
		{
			var u = Tenant.CreateUrl("Cryptography", "VerifyHash");
			var e = new JObject
			{
				{"value", value},
				{"existing", existing}
			};

			return Tenant.Post<bool>(u, e);
		}
	}
}
