using Newtonsoft.Json.Linq;

namespace TomPIT.Proxy.Remote
{
	internal class CryptographyController : ICryptographyController
	{
		private const string Controller = "Cryptography";
		public string Decrypt(string cipherText)
		{
			var u = Connection.CreateUrl(Controller, "Decrypt");
			var e = new JObject
			{
				{"cipherText", cipherText}
			};

			return Connection.Post<string>(u, e);
		}

		public string Encrypt(string plainText)
		{
			var u = Connection.CreateUrl(Controller, "Encrypt");
			var e = new JObject
			{
				{"plainText", plainText}
			};

			return Connection.Post<string>(u, e);
		}

		public string Hash(string value)
		{
			var u = Connection.CreateUrl(Controller, "Hash");
			var e = new JObject
			{
				{"value", value}
			};

			return Connection.Post<string>(u, e);
		}

		public bool VerifyHash(string value, string existing)
		{
			var u = Connection.CreateUrl(Controller, "VerifyHash");
			var e = new JObject
			{
				{"value", value},
				{"existing", existing}
			};

			return Connection.Post<bool>(u, e);
		}
	}
}
