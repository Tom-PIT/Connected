namespace TomPIT.Security
{
	public interface ISysCryptographyService
	{
		string Encrypt(object sender, string plainText);
		string Decrypt(object sender, string cipherText);

		string Encrypt(object sender, EncryptionEventArgs e);
		string Decrypt(object sender, EncryptionEventArgs e);

		string Hash(string value);
		bool VerifyHash(string value, string existing);
	}
}
