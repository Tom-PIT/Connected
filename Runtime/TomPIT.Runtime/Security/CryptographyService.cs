using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class CryptographyService : ICryptographyService
	{
		public CryptographyService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public string Decrypt(string cipherText)
		{
			var u = Connection.CreateUrl("Cryptography", "Decrypt");
			var e = new JObject
			{
				{"cipherText", cipherText}
			};

			return Connection.Post<string>(u, e);
		}

		public string Encrypt(string plainText)
		{
			var u = Connection.CreateUrl("Cryptography", "Encrypt");
			var e = new JObject
			{
				{"plainText", plainText}
			};

			return Connection.Post<string>(u, e);
		}

		public string Hash(string value)
		{
			var u = Connection.CreateUrl("Cryptography", "Hash");
			var e = new JObject
			{
				{"value", value}
			};

			return Connection.Post<string>(u, e);
		}

		public bool VerifyHash(string value, string existing)
		{
			var u = Connection.CreateUrl("Cryptography", "VerifyHash");
			var e = new JObject
			{
				{"value", value},
				{"existing", existing}
			};

			return Connection.Post<bool>(u, e);
		}
	}
}
