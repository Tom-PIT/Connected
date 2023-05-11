namespace TomPIT.Proxy
{
	public interface ICryptographyController
	{
		string Encrypt(string plainText);
		string Decrypt(string cipherText);

		string Hash(string value);
		bool VerifyHash(string value, string existing);
	}
}
