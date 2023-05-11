using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TomPIT.Security;

namespace TomPIT.Sys.Services
{
	internal class CryptographyService : ISysCryptographyService
	{
		private const string PassPhrase = "p7sfecd2t9l09mn9030j3ekl9i4rt6gt";
		private const string SaltValue = "l0stwqhjbf53ipss776189007sbgtww2";
		private const string HashAlgorithm = "SHA1";
		private const int PasswordIterations = 0x04;
		private const string InitVector = "1234567890abcdef";
		private const int KeySize = 0x100;

		public string Encrypt(object sender, string plainText)
		{
			if (string.IsNullOrWhiteSpace(plainText))
				return null;

			return Encrypt(sender, new EncryptionEventArgs
			{
				PassPhrase = PassPhrase,
				SaltValue = SaltValue,
				HashAlgorithm = HashAlgorithm,
				PasswordIterations = PasswordIterations,
				InitVector = InitVector,
				KeySize = KeySize,
				Value = plainText
			}
			);
		}

		public string Decrypt(object sender, string cipherText)
		{
			if (string.IsNullOrWhiteSpace(cipherText))
				return null;

			return Decrypt(sender, new EncryptionEventArgs
			{
				PassPhrase = PassPhrase,
				SaltValue = SaltValue,
				HashAlgorithm = HashAlgorithm,
				PasswordIterations = PasswordIterations,
				InitVector = InitVector,
				KeySize = KeySize,
				Value = cipherText
			});
		}

		public string Encrypt(object sender, EncryptionEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Value))
				return e.Value;

			var initVectorBytes = Encoding.ASCII.GetBytes(e.InitVector);
			var saltValueBytes = Encoding.ASCII.GetBytes(e.SaltValue);
			var plainTextBytes = Encoding.UTF8.GetBytes(e.Value);
			var password = new PasswordDeriveBytes(e.PassPhrase, saltValueBytes, e.HashAlgorithm, e.PasswordIterations);
			var keyBytes = password.GetBytes(e.KeySize / 8);
			var symmetricKey = new RijndaelManaged
			{
				Mode = CipherMode.CBC
			};

			var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

			using (var ms = new MemoryStream())
			{
				using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
				{
					cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
					cryptoStream.FlushFinalBlock();

					var cipherTextBytes = ms.ToArray();
					var cipherText = Convert.ToBase64String(cipherTextBytes);

					return cipherText;
				}
			}
		}

		public string Decrypt(object sender, EncryptionEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Value))
				return e.Value;

			var initVectorBytes = Encoding.ASCII.GetBytes(e.InitVector);
			var saltValueBytes = Encoding.ASCII.GetBytes(e.SaltValue);
			var cipherTextBytes = Convert.FromBase64String(e.Value);
			var password = new PasswordDeriveBytes(e.PassPhrase, saltValueBytes, e.HashAlgorithm, e.PasswordIterations);
			var keyBytes = password.GetBytes(e.KeySize / 8);
			var symmetricKey = new RijndaelManaged
			{
				Mode = CipherMode.CBC
			};

			var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

			using (var ms = new MemoryStream(cipherTextBytes))
			{
				using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
				{
					var plainTextBytes = new byte[cipherTextBytes.Length];
					var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
					var plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

					return plainText;
				}
			}
		}

		public string Hash(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;

			using (var md = MD5.Create())
			{
				return GetHash(md, value);
			}
		}

		private string GetHash(MD5 hash, string value)
		{
			var data = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
			var sb = new StringBuilder();

			for (var i = 0; i < data.Length; i++)
				sb.Append(data[i].ToString("x2"));

			return sb.ToString();
		}

		public bool VerifyHash(string value, string existing)
		{
			if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(existing))
				return true;

			using (var md = MD5.Create())
			{
				var hash = GetHash(md, value);
				var comparer = StringComparer.OrdinalIgnoreCase;

				return comparer.Compare(hash, existing) == 0;
			}
		}
	}
}
